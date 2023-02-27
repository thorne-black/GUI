using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WyldTerm_PC
{
    /// <summary>
    /// Interaction logic for Flash.xaml
    /// </summary>
    public partial class Flash : Page
    {
        public string picoFolder = @"D:\";
        public List<long> page_list = new List<long>();

        public Flash()
        {
            Loaded += Page_Loaded;
            InitializeComponent();
        }

        private void Check_Pico_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Check_Modem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Check_App_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Update_App_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            if (win.serial.IsOpen)
            {
                Update_Pico_Btn.IsEnabled = true;//in future, only if new versions available, need to check with hosting source
                Update_Modem_Btn.IsEnabled = false;//change to true when it's working
            }
        }
        private void Send_Command(string command)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Send_Command(command);
        }

        public async Task Delay_Task(int duration)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(duration);
            });
        }
        public async void Update_Pico_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("pico boot\0\r");
            await Delay_Task(2000);
            for (int loop_check = 0; loop_check != 5; loop_check++)
            {
                if (!Directory.Exists(picoFolder)) await Delay_Task(2000);
                else break;
            }
            if (!Directory.Exists(picoFolder))
            {
                Update_Status("Directory doesn't exist!");
                return;
            }
            Copy_File();
            Update_Status("Copying");
            string fileLocation = System.IO.Path.Combine(Retrieve_Setting("Pico_Firmware_Folder"), Retrieve_Setting("Pico_Firmware_Filename"));
            string dest_file = System.IO.Path.Combine(picoFolder, Retrieve_Setting("Pico_Firmware_Filename"));
            System.IO.File.Copy(fileLocation, dest_file, true);
            await Delay_Task(2000);
            for (int loop_check = 0; loop_check != 5; loop_check++)
            {
                if (Directory.Exists(picoFolder)) await Delay_Task(2000);
                else break;
            }
            if (Directory.Exists(picoFolder))
            {
                Update_Status("Issue saving file to Pico");
                return;
            }
            else
            {
                Update_Status("Update complete!");
                await Delay_Task(2000);
                MainWindow win = (MainWindow)Window.GetWindow(this);
                if (!win.serial.IsOpen) win.OpenPort(win.ComPorts.Text, win.Baudrates.Text);
                else Update_Status("Reconnected!");
            }
        }

        public async void Copy_File()
        {
            Update_Status("Copying");
            string fileLocation = System.IO.Path.Combine(Retrieve_Setting("Pico_Firmware_Folder"), Retrieve_Setting("Pico_Firmware_Filename"));
            string dest_file = System.IO.Path.Combine(picoFolder, Retrieve_Setting("Pico_Firmware_Filename"));
            System.IO.File.Copy(fileLocation, dest_file, true);
            await Delay_Task(2000);
            for (int loop_check = 0; loop_check != 5; loop_check++)
            {
                if (Directory.Exists(picoFolder)) await Delay_Task(2000);
                else break;
            }
            if (Directory.Exists(picoFolder))
            {
                Update_Status("Issue saving file to Pico");
                return;
            }
            else
            {
                Update_Status("Update complete!");
                MainWindow win = (MainWindow)Window.GetWindow(this);
                win.OpenPort(win.ComPorts.Text, win.Baudrates.Text);
            }
        }
        private void Update_Status(string message)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Warning_Status(message);
        }
        public string Retrieve_Setting(string setting_name)
        {
            System.Configuration.ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = "C:\\Users\\katel\\Documents\\code\\Test_Terminal\\WyldTerm_PC\\App.Config";
            Configuration configFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            return settings[setting_name].Value;
        }

        private void Update_Modem_Click(object sender, RoutedEventArgs e)
        {
            string[] file_array = Pre_Process_SREC();
            Update_Status($"Lines: \"{file_array.Length}\"");
            if (file_array.Length == 0) return;
            Send_Command("Modem Boot");
            Send_Lines_To_Pico(file_array);
        }

        private string[] Pre_Process_SREC()
        {
            string[] file_array = { };
            string location = Locate_SREC_File();
            if (location.Length == 0) return file_array;
            page_list.Clear();
            file_array = File.ReadAllLines(location);
            Page_List(file_array);
            string path = @"C:\Users\katel\Documents\test_file.txt";
            if (!File.Exists(path)) File.CreateText(path);
            File.WriteAllLines(path, file_array);
            long[] page_array = page_list.ToArray();
            string.Join(",", page_array);
            string[] result = Array.ConvertAll(page_array, x => x.ToString());
            File.WriteAllLines(path, result);
            return file_array;
        }

        private string Locate_SREC_File()
        {
            string folder_path = Retrieve_Setting("Modem_Firmware_Folder");
            string source_filename = Retrieve_Setting("Modem_Firmware_Filename");
            if (!File.Exists(Retrieve_Setting("Modem_Firmware_Folder") + Retrieve_Setting("Modem_Firmware_Filename")))
            {
                Update_Status("Error locating source");
                return "";
            }
            else
            {
                Update_Status("File found!");
                return folder_path + source_filename;
            }
        }

        private void Page_List(string[] file_array)
        {
            foreach (string line in file_array)
            {
                Find_Pages(line);
            }
        }

        public void Find_Pages(string line)
        {
            string record_type = line.Substring(0, 2);
            string address = "";
            if (record_type.Equals("S1"))
            {
                address = line.Substring(4, 4);

            }
            else if (record_type.Equals("S2"))
            {
                address = line.Substring(4, 6);

            }
            else if (record_type.Equals("S3"))
            {
                address = line.Substring(4, 8);
            }
            else
            {
                return;
            }
            //int address_len = address.Length;
            //int data_len = line.Length - (address_len + 4);
            long hexNum = long.Parse(address, System.Globalization.NumberStyles.HexNumber);
            long page = hexNum / 0x2000;
            // long len = (hexNum + (line.Length / 2));
            //long len2 = (page + (line.Length / 2));
            int len_data = line.Length - (address.Length + 4);
            int size = (len_data / 2);
            long data_addr = ((hexNum + size - 1) / 0x2000);
            //line_.Add("page: " + page + " len_data: " + len_data + " size: " + size + " data_addr: " + data_addr);
            if (!page_list.Contains(page)) page_list.Add(page);
            if (!page_list.Contains(data_addr)) page_list.Add(data_addr);
        }
        private void Send_Lines_To_Pico(string[] file_array)
        {
            int num_pages = Separate_Data_Into_Pages(file_array);
            Erase_Pages();
            Send_Pages(file_array);
        }

        private int Separate_Data_Into_Pages(string[] file_array)
        {
            int total_length = 0;
            foreach (string line in file_array)
            {
                total_length += line.Length;
            }
            return total_length / 255;
        }

        private async void Erase_Pages() {
            foreach (long page in page_list)
            {
                string hexValue = (page*0x2000).ToString("X");
                Send_Command("P");
                await Delay_Task(100);
                Send_Command($"{hexValue}");
                //send erase command for (page)
            }
        }

        private bool Send_Pages(string[] file_array)
        {
            foreach (string line in file_array)
            {
                //Send_Command($"P {page * 0x2000}");
                //send erase command for (page)
            }
            return true;
        }

        private void Help_Check_Pico_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Check for more recent versions of the Pico program. Not yet available.");
        }

        private void Help_Update_Click(object sender, RoutedEventArgs e)
        {
            string fileLocation = System.IO.Path.Combine(Retrieve_Setting("Pico_Firmware_Folder"), Retrieve_Setting("Pico_Firmware_Filename"));
            Update_Help($"Auto add the latest Pico program to the Pico, located at: {fileLocation}.\nThe filename and folder can be changed in the \"App Settings\" tab.\nThis will take place automatically if permission is given in the \"App Settings\" tab.");
        }

        private void Help_Check_Modem_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Check for more recent versions of the Modem firmware. Not yet available.");
        }

        private void Help_Update_Modem_Click(object sender, RoutedEventArgs e)
        {
            string fileLocation = System.IO.Path.Combine(Retrieve_Setting("Modem_Firmware_Folder"), Retrieve_Setting("Modem_Firmware_Filename"));
            Update_Help($"Development in progress, currently unavailable.\nAuto add the latest Pico program to the Pico, located at: {fileLocation}.\nThe filename and folder can be changed in the \"App Settings\" tab.\nThis will take place automatically if permission is given in the \"App Settings\" tab.");
        }

        private void Help_Check_App_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Check for more recent versions of the Modem firmware. Not yet available.");
        }

        private void Help_Update_App_Click(object sender, RoutedEventArgs e)
        {
            Update_Help($"Auto update this app. Not yet available.");
        
        }

        private void Update_Help(string message)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Help_Text_Update(message);
        }
    }
}
        /*

        def command(s, command, reply):
    #print("> {}".format(command))
    command += "\n"
    s.flushInput()
    s.flushOutput()
    s.write(command.encode('ascii'))
    rx = s.read_until(reply.encode('ascii'))
    #print("< {}".format(rx.decode('ascii', 'replace')))
    return reply.encode('ascii') in rx

            command(s, 'P {}'.format(hex(page* page_size)), 'OK\r\nULDR>'):
        private bool Send_Pages(int num_pages)
        {
            foreach(long page in page_list)
            {

            }

            return true;
        }
    }*/