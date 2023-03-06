using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
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
    /// Interaction logic for Log.xaml
    /// </summary>
    public partial class Log : Page
    {
        public int indexStart;
        public int indexPenultimate;
        public int placeholder_ready;

        public Log()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            if (win.serial.IsOpen)
            {
                View_Beacons.IsEnabled = true;
                Log_Data.IsEnabled = true;
                Erase_Data.IsEnabled = true;
                Log_Session.IsEnabled = true;
                Clear_Session.IsEnabled = true;
            }
            Save_Text_Box.Document.Blocks.Clear();
        }

        private void View_Beacons_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("Display Data");
        }

        private async void Log_Data_Click(object sender, RoutedEventArgs e)
        {
            Save_Text_Box.Document.Blocks.Clear();
            await Wait_For_Data_Display_Completion();
            View_Data_Click(sender, e);
        }

        private void Erase_Data_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("Erase Data");
        }

        private void Log_Session_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            TextRange output = new TextRange(win.Commdata.Document.ContentStart, win.Commdata.Document.ContentEnd);
            string entire_output = output.Text;
            string folder = Retrieve_Setting("Beacon_Data_Folder");
            SaveFile(folder, "LogFile", entire_output);
            win.Warning_Status("Data Saved!");
            View_Data_Click(sender, e);
        }

        private void Clear_Session_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Commdata.Document.Blocks.Clear();
        }

        private void View_Data_Click(object sender, RoutedEventArgs e)
        {
            Save_Text_Box.Document.Blocks.Clear();
            var beacon_directory = new DirectoryInfo(Retrieve_Setting("Beacon_Data_Folder"));
            var latest_file = beacon_directory.GetFiles()
             .OrderByDescending(f => f.LastWriteTime)
             .First();

            Save_Text_Box.BeginChange();
            foreach (var line in File.ReadAllLines(latest_file.FullName))
            {
                Save_Text_Box.Selection.Text += line + "\0\r";
            }
            Save_Text_Box.EndChange();
        }

        private void Erase_File_Click(object sender, RoutedEventArgs e)
        {
            var beacon_directory = new DirectoryInfo(Retrieve_Setting("Beacon_Data_Folder"));
            var latest_file = beacon_directory.GetFiles()
             .OrderByDescending(f => f.LastWriteTime)
             .First();
            latest_file.Delete();
            Save_Text_Box.Document.Blocks.Clear();
        }


        private void Send_Command(string command)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Send_Command(command);
        }

        public async Task Wait_For_Data_Display_Completion()
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            Send_Command("Display Data");
            await Task.Delay(1000);
            int output_len_before = 0;
            int output_len_after = 1;
            while (output_len_before != output_len_after) { 
                TextRange output_before = new TextRange(win.Commdata.Document.ContentStart, win.Commdata.Document.ContentEnd);
                output_len_before = output_before.Text.Length;
                await Task.Delay(2000);
                TextRange output_after = new TextRange(win.Commdata.Document.ContentStart, win.Commdata.Document.ContentEnd);
                output_len_after = output_after.Text.Length;
                if (output_len_before == output_len_after)
                {
                    Parse_Data();
                    return;
                }//if there's a 2 second delay with nothing printing, it's finished
            }
        }

        private void Parse_Data()
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            string data_only = Search_And_Filter_Data();
            string folder = Retrieve_Setting("Beacon_Data_Folder");
            string file = Retrieve_Setting("Beacon_Data_Filename");
            if (data_only == "Start issue" || data_only == "End issue" || data_only.Length < 5)
            {
                win.Warning_Status("Data Not found!");
                return;
            }
            SaveFile(folder, file, data_only);
            win.Warning_Status("Data Saved!");
        }


        public void SaveFile(string folder, string file, string data_only)

        {
            string today = DateTime.Now.ToString(".H.m.s~d-M-yy");
            string fileName = (file + today + ".txt");
            string pathString = System.IO.Path.Combine(folder, fileName);
            using (FileStream fs = new FileStream(pathString, FileMode.Create))
            { 
                Byte[] title = new UTF8Encoding(true).GetBytes(fileName + "\n");
                fs.Write(title, 0, title.Length);
                Byte[] beaconData = new UTF8Encoding(true).GetBytes(data_only);
                fs.Write(beaconData, 0, beaconData.Length);
            }

        }
        public string Search_And_Filter_Data() //issues with retrieving rich text value.
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            TextRange output = new TextRange(win.Commdata.Document.ContentStart, win.Commdata.Document.ContentEnd);
            string entire_output = output.Text;
            string data_only;
            int start = entire_output.LastIndexOf("Display Data", StringComparison.OrdinalIgnoreCase);
            if (start == -1)
            {
                win.Warning_Status("Start");
                return "Start issue";
            }

            int end = entire_output.LastIndexOf("Please");
            if (end == -1)
            {
                win.Warning_Status("End");
                return "End issue";
            }
            {
                data_only = entire_output.Substring(start, (end - start));
            }
            return data_only;
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
