using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Metrics;
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
    /// Interaction logic for Configs.xaml
    /// </summary>
    public partial class Configs : Page
    {
        public List<string> parameters = new List<string>();
        public Configs()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            string uplinkID = Retrieve_Setting("Uplink_ID");
            UplinkID.BeginChange();
            UplinkID.Selection.Text = uplinkID;
            UplinkID.EndChange();
            MainWindow win = (MainWindow)Window.GetWindow(this);
            if (win.serial.IsOpen)
            {
                Save_Config_Uplink.IsEnabled = true;
                Save_Config_Sniff.IsEnabled = true;
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

        private void UplinkID_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private bool Check_Values_Uplink()
            
        {
            parameters.Clear();
            if (config_name_Up.Text.Length == 0 || config_name_Up.Text.Length > 8)
            {
                Config_Name_Up.Foreground = Brushes.Red;
                return false;
            }
            else
            {
                parameters.Add(config_name_Up.Text);
            }

            int up_pwr = int.Parse(uplink_power.Text);
            if (up_pwr < 0 || up_pwr > 22)
            {
                Uplink_Power.Foreground = Brushes.Red;
                return false;
            }
            float freq = float.Parse(frequency.Text);
            int freqHz = ((int)freq * 1000000);
            if (freqHz < 868000000 || freqHz > 869900000)
            {
                Frequency_Label.Foreground = Brushes.Red;
                return false;
            }
            else
            {
                parameters.Add($"{up_pwr}" + " " + $"{freqHz}");
            }
            if (Selant_sm_Up.IsChecked == true) parameters.Add("S M");
            else if (Selant_ms_Up.IsChecked == true) parameters.Add("M S");
            else if (Selant_ss_Up.IsChecked == true) parameters.Add("S S");
            else if (Selant_mm_Up.IsChecked == true) parameters.Add("M M");
            else
            {
                Selant_Up.Foreground = Brushes.Red;
                return false;
            }
            parameters.Add("3");//autorx - will need removing because always needed

            if (Cal_True_Up.IsChecked == true) parameters.Add("043300000");
            else if (Cal_False_Up.IsChecked == true) parameters.Add("False");
            else {
                Calimage_Up.Foreground = Brushes.Red;
                return false;
            }
            parameters.Add("1");//rxgain - true

            if (Ext_True_Up.IsChecked == true) parameters.Add("1");
            else if (Ext_False_Up.IsChecked == true) parameters.Add("0");
            else
            {
                Extlna_Up.Foreground = Brushes.Red;
                return false;
            }
            parameters.Add("3");//scodingrate - always 3
            parameters.Add("01");//setautouplink 01 - true for uplink

            if (UplinkID.Selection.Text != "Not Set")
            {
                parameters.Add(UplinkID.Selection.Text);
            }
            else
            {
                Uplink.Foreground = Brushes.Red;
                return false;
            }
            if (SF_7_Up.IsChecked == true) parameters.Add("6");
            else if (SF_10_Up.IsChecked == true) parameters.Add("5");
            else
            {
                SF_Up.Foreground = Brushes.Red;
                return false;
            }
            if (Default_True_Up.IsChecked == true) parameters.Add("Yes");
            else if (Default_False_Up.IsChecked == true) parameters.Add("No");
            else
            {
                Default_Up.Foreground = Brushes.Red;
                return false;
            }
            return true;
        }

        private bool Check_Values_Sniff()

        {
            parameters.Clear();
            if (config_name_S.Text.Length == 0 || config_name_S.Text.Length > 8)
            {
                Config_Name_S.Foreground = Brushes.Red;
                return false;
            }
            else
            {
                parameters.Add(config_name_S.Text);
            }
            if (Selant_sm_S.IsChecked == true) parameters.Add("S M");
            else if (Selant_ms_S.IsChecked == true) parameters.Add("M S");
            else if (Selant_ss_S.IsChecked == true) parameters.Add("S S");
            else if (Selant_mm_S.IsChecked == true) parameters.Add("M M");
            else
            {
                Selant_S.Foreground = Brushes.Red;
                return false;
            }
            parameters.Add("3");//autorx 3

            if (Cal_True_S.IsChecked == true) parameters.Add("043300000");
            else if (Cal_False_S.IsChecked == true) parameters.Add("False");
            else
            {
                Calimage_S.Foreground = Brushes.Red;
                return false;
            }
            parameters.Add("1");//rxgain - true

            if (Ext_True_S.IsChecked == true) parameters.Add("1");
            else if (Ext_False_S.IsChecked == true) parameters.Add("0");
            else
            {
                Extlna_S.Foreground = Brushes.Red;
                return false;
            }
            parameters.Add("0");//setautouplink 01 - false for sniff

            if (SF_7_S.IsChecked == true) parameters.Add("6");
            else if (SF_10_S.IsChecked == true) parameters.Add("5");
            else
            {
                SF_S.Foreground = Brushes.Red;
                return false;
            }
            if (Default_True_S.IsChecked == true) parameters.Add("Yes");
            else if (Default_False_S.IsChecked == true) parameters.Add("No");
            else
            {
                Default_S.Foreground = Brushes.Red;
                return false;
            }
            return true;
        }

        private void Send_Command(string command)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Send_Command(command);
        }

        private void Save_Config_Uplink_Click(object sender, RoutedEventArgs e)
        {
            bool check = Check_Values_Uplink();
            if (!check)
            {
                MainWindow win = (MainWindow)Window.GetWindow(this);
                win.Warning_Status("Value out of bounds, not saved!");
            }
            else
            {
                Send_Config_Any();

            }
        }

        private void Save_Config_Sniff_Click(object sender, RoutedEventArgs e)
        {
            bool check = Check_Values_Sniff();
            if (!check)
            {
                MainWindow win = (MainWindow)Window.GetWindow(this);
                win.Warning_Status("Value out of bounds, not saved!");
            }
            else
            {
                Send_Config_Any();

            }
        }

        private async void Send_Config_Any()
        {
            string parameter_str = "";
            foreach (string param in parameters)
            {
                parameter_str += (param + ", ");
            }
            string parameters_trim = parameter_str.Remove(parameter_str.Length -2, 2);
            Send_Command("Add Config, " + parameters_trim);
        }

        public async Task Delay_Task(int duration)
        {
            await Task.Run(async () =>
            {
                await Task.Delay(duration);
            });
        }
    }
}
