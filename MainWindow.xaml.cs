using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace WyldTerm_PC
{
    public partial class MainWindow : Window
    {

        public SerialPort serial = new SerialPort();
        string results;
        public Match m;
        public string[] Ports = new string[] { };

        public MainWindow()
        {
            Loaded += Window_Loaded;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Update_Ports();
            List<string> All_Baudrates = new List<string> { "2400", "9600", "19200", "38400", "57600", "115200", "230400", "460800", "921600" };
            Baudrates.ItemsSource = All_Baudrates;
            Baudrates.SelectedItem = All_Baudrates[5];
        }

        public string[] Update_Ports()
        {
            Ports = SerialPort.GetPortNames();
            ComPorts.ItemsSource = Ports;
            if (Ports.Length > 0)
            {
                ComPorts.SelectedItem = Select_Port(Ports);
                Connect.IsEnabled = true;
                Disconnect.IsEnabled = false;
            
            }
            else
            {
                ComPorts.SelectedIndex = 0;
                //ComPorts.SelectedIndex = -1;
                //ComPorts.SelectedItem = "";
            }
            return Ports;
        }


        public string Select_Port(string[] Ports)
        {
            bool match_port = false;
            string Port_Name = "";
            String default_port = Retrieve_Setting("Port");
            match_port = false;
            for (int i = 0; i != Ports.Count(); i++)
            {
                int compare_name = String.Compare(default_port, Ports[i]);
                if (compare_name == 0)
                {
                    Port_Name = Ports[i];
                    match_port = true;
                    return Port_Name;
                }
            }
            if (!match_port)
            {
                Port_Name = Ports[0];
            }
            return Port_Name;
        }

        private void Submit_Command_Click(object sender, RoutedEventArgs e)
        {
            Send_Command(Command_Box.Text);
            Command_Box.Text = "";
        }


        private void Connected_Box_TextChanged(object sender, TextChangedEventArgs e)
        {
            Connected_Box.ScrollToEnd();
        }

        private void Commdata_TextChanged(object sender, TextChangedEventArgs e)
        {
            Commdata.ScrollToEnd();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        //CONNECTION FUNCTIONS
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            OpenPort(ComPorts.Text, Baudrates.Text);
        }

        private void Default_Connect_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Help_Connect_Click(object sender, RoutedEventArgs e)
        {
            Help_Text_Update("Select the port and baudrate from the dropdowns.\nTo find out more about these, click the \"Tutorials\" button.\nClick \"Connect\" to connect with these values.\nTo save time, save your default COM port and baudrate in \"App Settings\".\nIf it is available, your default port will be auto selected from the list. If none are showing, the Pico is not plugged in, in bootloader mode, or malfunctioning.\nThe Pico baudrate of \"112500\" is auto selected in the dropdown, though others are available in case you want to use this terminal program with another device.");
        }

        private void Help_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Help_Text_Update("Use this to disconnect from the Pico.\nYou may wish to use this to swap between devices.\nYou may also wish to do this if you unplug the Pico, as this app does not yet subscribe to USB events (though the connection status will be checked and updated prior to performing new actions with the Pico).");
        }

        public void Disconnect_Port()
        {
            if (serial.IsOpen)
            {
                 serial.Close();
            }

            Disconnect.IsEnabled = false;
            Sleep.IsEnabled = false;
            Wake.IsEnabled = false;
            Menu.IsEnabled = false;
            Next_Menu.IsEnabled = false;
            Update_Ports();
        }

        public void OpenPort(string portStr, string baudStr)
        {
            this.Dispatcher.Invoke(() =>
            {
                if (portStr.Length == 0)
                {
                    return;
                }
                serial.PortName = portStr;
                serial.BaudRate = Convert.ToInt32(baudStr);
                serial.Handshake = System.IO.Ports.Handshake.None;
                serial.Parity = Parity.None;
                serial.DataBits = 8;
                serial.StopBits = StopBits.One;
                serial.ReadTimeout = 200;
                serial.WriteTimeout = 50;
                string[] Ports;
                Ports = SerialPort.GetPortNames();
                if (Ports.Length == 0)
                {
                    Warning_Status("No port Selected!");
                    return;
                }
                bool match_port = false;
                for (int i = 0; i != Ports.Count(); i++)
                {
                    int compare_name = String.Compare(portStr, Ports[i]);
                    if (compare_name == 0)
                    {
                        match_port = true;
                        break;
                    }
                }

                if (!match_port)
                {
                    Connect.IsEnabled = false;
                    Warning_Status("Port selected is unavailable");
                    return;
                }

                Thread open = new Thread(new ThreadStart(Open_Port));
                open.Start();
                Thread.Sleep(500);

                if (serial.IsOpen)
                {
                    Connect.IsEnabled = false;
                    Disconnect.IsEnabled = true;
                    Sleep.IsEnabled = true;
                    Wake.IsEnabled = true;
                    Menu.IsEnabled = true;
                    ComPorts.SelectedItem = "";
                    Connected_Status("Connected!");
                }
                serial.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Recieve);
            });
        }

        private void Open_Port()
        {
            try
            {
                serial.Open();
                serial.DtrEnable = true;
                serial.DiscardInBuffer();
            }
            catch (Exception)
            {
                Warning_Status("Error, Pico may be asleep! Unplug and replug the Pico");
                return;
            }

        }

        public void Recieve(object sender, System.IO.Ports.SerialDataReceivedEventArgs e)
        {

            this.Dispatcher.Invoke(() =>
            {
                results += serial.ReadExisting();
                FlowDocument document = new FlowDocument();
                Paragraph paragraph = new Paragraph();
                {
                    paragraph.Inlines.Add(results);
                    document.Blocks.Add(paragraph);
                    Commdata.Document = document;
                    Commdata.ScrollToEnd();
                }
                if (Search_For_String("Going to sleep!"))
                {
                    Warning_Status("Device sleeping, waking up!");
                }
            });
        }

        public void Send_Command(string data)
        {
            if (serial.IsOpen)
            {
                try
                {
                    data = (data + "\0\r");
                    byte[] hexstring = Encoding.ASCII.GetBytes(data);
                    foreach (byte hexval in hexstring)
                    {
                        {
                            byte[] _hexval = new byte[] { hexval };
                            serial.Write(_hexval, 0, 1);
                            //  serial.Write(SerialData.Text);
                            Thread.Sleep(1);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Warning_Status("Failed to SEND" + data + "\n" + ex + "\n");
                    Disconnect_Port();

                }
            }
            else
            {
                Disconnect_Port();
                Warning_Status("Not connected!");
            }
        }

        public void Connected_Status(string status)
        {
            Connected_Box.BeginChange();
            Connected_Box.Selection.Text = status;
            Connected_Box.EndChange();
        }

        public void Warning_Status(string warning)
        {
            this.Dispatcher.Invoke(() =>
            {
                Warning_Box.BeginChange();
                Warning_Box.Document.Blocks.Clear();
                Warning_Box.Selection.Text = warning;
                Warning_Box.EndChange();
            });
        }

        public bool Search_For_String(string search_str)
        {
            string latest_string = Detect_Ready();
            m = Regex.Match(latest_string, search_str, RegexOptions.IgnoreCase);
            if (!m.Success)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public string Detect_Ready()
        {
            TextRange result = new TextRange(Commdata.Document.ContentStart, Commdata.Document.ContentEnd);
            string dataMatch = "Please enter a command.";
            string resultText = result.Text;
            int indexStart = resultText.LastIndexOf(dataMatch);
            if (indexStart > 0)
            {
                int indexPenultimate = resultText.LastIndexOf(dataMatch, (indexStart - 1));
                if (indexPenultimate > 0)
                {
                    int indexEnd = resultText.Length;
                    string latestOutput = resultText.Substring(indexPenultimate, indexEnd - indexPenultimate);
                    return latestOutput;
                }
                else
                {
                    return "First";
                }
            }
            else
            {
                return "No Occurences";
            }

        }

        public int First_Index_Of(string latest_string, string search_str)
        {
            return latest_string.IndexOf(search_str);
        }

        public int Last_Index_Of(string latest_string, string search_str)
        {
            return latest_string.LastIndexOf(search_str);
        }

        private void ComPorts_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ComPorts.ItemsSource = "";
            ComPorts.SelectedItem = "";
            ComPorts.SelectedIndex = -1;
            Update_Ports();
        }

        public void Help_Text_Update(string help_text)
        {
            Help_Box.IsExpanded = true;
            Help_Text.BeginChange();
            Help_Text.Selection.Text = help_text;
            Help_Text.EndChange();
        }


        public string Retrieve_Setting(string setting_name)
        {
            System.Configuration.ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = "C:\\Users\\katel\\Documents\\code\\Test_Terminal\\WyldTerm_PC\\App.Config";
            Configuration configFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            return settings[setting_name].Value;
        }

        private void Sleep_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("Dormant");
        }

        private void Wake_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("Rise and shine!");
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("Menu");
            Next_Menu.IsEnabled = true;
            Menu.IsEnabled = false;

        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("\r");
            Next_Menu.IsEnabled = false;
            Menu.IsEnabled = true;
        }

        private void Help_Sleep_Click(object sender, RoutedEventArgs e)
        {
            Help_Text_Update("Click \"Pico Sleep\" to put the device into dormant mode. Use \"Pico Wake\" when the device is connected but has gone to sleep.\nIf the Pico goes to sleep before connecting to it, you will need to physically disconnect it from the USB port, then reconnect.");
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Help_Text.Document.Blocks.Clear();
            Warning_Box.Document.Blocks.Clear();
            Help_Box.IsExpanded = false;
        }

        private void Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Disconnect_Port();
        }

        private void Help_Menu_Click(object sender, RoutedEventArgs e)
        {
            Help_Text_Update("Use \"Menu\" to view all available commands. The menu is split over two pages, click \"Next\" to view the second page.");
        }

        private void Tutorials_Click(object sender, RoutedEventArgs e)
        {
            Tutorial tutorial_window = new Tutorial();
            tutorial_window.Show();
        }
    }
}
