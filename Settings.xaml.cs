using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
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
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Page
    {
        public String[] Ports = new String[] { };
        public Settings()
        {
            Loaded += Page_Loaded;
            InitializeComponent();
        }

        private void Com_Defaults_Click(object sender, RoutedEventArgs e)
        {
            string port_default = Retrieve_Setting("Port");
            string baud_default = Retrieve_Setting("Baud");
            Update_Help($"This sets the default values for COM Port and Baudrate, so that they are selected (if available) at app startup.\nThe default COM Port is {port_default} and the default Baudrate is {baud_default}.");
        }

        private void Data_Filename_Default_Click(object sender, RoutedEventArgs e)
        {
            string data_filename = Retrieve_Setting("Beacon_Data_Filename");
            Update_Help($"This is the name of the file the beacon data will be stored in. The default is \"{data_filename}\"");
        }

        private void Data_Dest_Default_Click(object sender, RoutedEventArgs e)
        {
            string data_folder = Retrieve_Setting("Beacon_Data_Folder");
            Update_Help($"This is the path to the folder the beacon data will be stored in. The default is \"{data_folder}\"");
        }

        private void ID_Default_Click(object sender, RoutedEventArgs e)
        {
            string uplink_ID = Retrieve_Setting("Uplink_ID");
            Update_Help($"This is your unique ID for when you uplink during beacon sniffing.\nClick the \"Eeserved IDs\" link to see which IDs are already in use.\nThe current default ID is: \"{uplink_ID}\"");
        }

        private void Firmware_Filename_Default_Click(object sender, RoutedEventArgs e)
        {
            string modem_filename = Retrieve_Setting("Modem_Firmware_Filename");
            Update_Help($"This is the Modem firmware filename, used to locate the file prior to upload. The default is \"{modem_filename}\"");
        }

        private void Firmware_Dest_Default_Click(object sender, RoutedEventArgs e)
        {
            string modem_folder = Retrieve_Setting("Modem_Firmware_Folder");
            Update_Help($"This is the path to the folder the Modem firmware is stored in. The default is \"{modem_folder}\"");
        }

        private void Pico_Filename_Default_Click(object sender, RoutedEventArgs e)
        {
            string pico_filename = Retrieve_Setting("Pico_Firmware_Filename");
            Update_Help($"This is the filename of the Pico software. The default is \"{pico_filename}\"");
        }

        private void Pico_Dest_Default_Click(object sender, RoutedEventArgs e)
        {
            string pico_folder = Retrieve_Setting("Pico_Firmware_Folder");
            Update_Help($"This is the path to the folder the Pico software is stored in. The default is \"{pico_folder}\"");
        }

        private void Compiler_Default_Click(object sender, RoutedEventArgs e)
        {
            string compiler_path = Retrieve_Setting("Compiler_Path");
            Update_Help($"This is the path to the compiler program on your computer.\nSee \"Tutorials\" for information on installing this. The default path is: \"{compiler_path}\"");
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            Ports = win.Update_Ports();
            Com_Port_New.ItemsSource = Ports;
            if (Ports.Length > 0)
            {
                Com_Port_New.SelectedItem = win.Select_Port(Ports);
            }
            List<string> All_Baudrates = new List<string> { "2400", "9600", "19200", "38400", "57600", "115200", "230400", "460800", "921600" };
            Baud_Rates_New.ItemsSource = All_Baudrates;
            Baud_Rates_New.SelectedItem = All_Baudrates[5];
            Initial_Radio();
        }

        public void AddUpdateAppSettings(string key, string value)
        {
            try
            {

                System.Configuration.ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
                fileMap.ExeConfigFilename = "C:\\Users\\katel\\Documents\\code\\Test_Terminal\\WyldTerm_PC\\App.Config";
                Configuration configFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                settings.Add(key, value);
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                Update_Status("Locating error!");
            }
        }

        private void Pico_Filename_Btn_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Pico_Firmware_Filename", pico_firmware_filename.Text);
            Update_Status("Saved!");
        }

        private void Pico_Dest_Btn_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Pico_Firmware_Folder", pico_firmware_folder.Text);
            Update_Status("Saved!");
        }

        private void Compiler_Btn_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Compiler_Path", compiler_path.Text);
            Update_Status("Saved!");
        }

        private void Com_Save_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Port", Com_Port_New.Text);
            AddUpdateAppSettings("Baud", Baud_Rates_New.Text);
            
            Update_Status("Saved!");
        }

        private void Data_Filename_Btn_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Beacon_Data_Filename", beacon_filename.Text);
            Update_Status("Saved!");
        }

        private void Data_Dest_Btn_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Beacon_Data_Folder", save_beacon_folder.Text);
            Update_Status("Saved!");
        }

        private void Firmware_Filename_Btn_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Modem_Firmware_Filename", modem_firmware_filename.Text);
            Update_Status("Saved!");
        }

        private void Firmware_Dest_Btn_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Modem_Firmware_Folder", modem_firmware_folder.Text);
            Update_Status("Saved!");
        }

        private void ID_Btn_Click(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Uplink_ID", save_uplinkid.Text);
            Update_Status("Saved!");
        }

        private void Update_Status(string message)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Warning_Status(message);
        }

        private void Update_Help(string message)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Help_Text_Update(message);
        }

        private void App_Permission_True(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Update_App", "True");
            App_True.IsChecked = true;
            App_False.IsChecked = false;
        }

        private void App_Permission_False(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Update_App", "False");
            App_True.IsChecked = false;
            App_False.IsChecked = true;
        }
        private void Firmware_Permission_True(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Update_Firmware", "True");
            Firmware_True.IsChecked = true;
            Firmware_False.IsChecked = false;
        }
        private void Firmware_Permission_False(object sender, RoutedEventArgs e)
        {
            AddUpdateAppSettings("Update_Firmware", "False");
            Firmware_True.IsChecked = false;
            Firmware_False.IsChecked = true;
        }

        public void Initial_Radio()
        {
           
            string app = Retrieve_Setting("Update_App");
            string firmware = Retrieve_Setting("Update_Firmware");
            if (app == "False")
            {
                App_True.IsChecked = false;
                App_False.IsChecked = true;
            }
            else
            {
                App_True.IsChecked = true;
                App_False.IsChecked = false;
            }
            if (firmware == "False")
            {
                Firmware_True.IsChecked = false;
                Firmware_False.IsChecked = true;
            }
            else
            {
                Firmware_True.IsChecked = true;
                Firmware_False.IsChecked = false;
            }

        }
        public string Retrieve_Setting(string setting_name)
        {
            System.Configuration.ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = "C:\\Users\\katel\\Documents\\code\\Test_Terminal\\WyldTerm_PC\\App.Config";
            Configuration configFile = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            var settings = configFile.AppSettings.Settings;
            return settings[setting_name].Value;
        }

    }
}
