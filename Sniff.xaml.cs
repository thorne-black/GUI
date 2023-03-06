using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.Arm;
using System.Security.Claims;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml.Linq;
using Xceed.Wpf.Toolkit;


namespace WyldTerm_PC

{
    public partial class Sniff : Page
    {


        public Sniff()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
           
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            Config_Name.Text = "Default";
            Duration.Text = "Default";
            Delay.Text = "0";
            
            MainWindow win = (MainWindow)Window.GetWindow(this);
            if (win.serial.IsOpen)
            {
                Start_Sniff.IsEnabled = true;
                Display_Default.IsEnabled = true;
                Change_Default.IsEnabled = true;
                Change_Duration.IsEnabled = true;
            }
            List<string> Configs = new List<string> { "Sniff", "Uplink" };
            Config_Types.ItemsSource = Configs;
            Config_Types.SelectedItem = Configs[0];

  
        }
        private void Warning_Status(string warning)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Warning_Status(warning);
        }

        private void Update_Help(string help)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Help_Text_Update(help);
        }

        private void Start_Sniff_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            bool check = Check_Values();
            if (check)
            {
                
                win.Warning_Status("Value(s) out of bounds, sniff not started!");

            }
            else
            {
                string sniff_cmd = "";
                if (Type_S.IsChecked == true) sniff_cmd = "Sniff";
                else sniff_cmd = "Uplink";
                string sniff_mode = "";
                if (Mode_I.IsChecked == true) sniff_mode = "True";
                else sniff_mode = "False";
                win.Send_Command(sniff_cmd + ", " + Config_Name.Text + ", " + Duration.Text + ", " + Delay.Text + ", " + sniff_mode);
                Stop_Sniff.IsEnabled = true;
                Start_Sniff.IsEnabled = false;
            }
        }

        private void Stop_Sniff_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("!");
            Stop_Sniff.IsEnabled = false;
            Start_Sniff.IsEnabled = true;
        }

        private bool Check_Values()
        {
            if (Config_Name.Text.Length < 0 || Config_Name.Text.Length > 8) return false;
            int compare_dur = String.Compare(Duration.Text, "Default", true);
            if (compare_dur != 0)
            {
                int Dur = int.Parse(Duration.Text);
                if (Dur < 1 || Dur > 4320) return true;
            }
            int Del = int.Parse(Delay.Text);
            if (Del < 0 || Del > 4320) return true;
            if (Type_S.IsChecked == false && Type_U.IsChecked == false) return true;
            if (Mode_C.IsChecked == false && Mode_I.IsChecked == false) return true;
            return false;
        }


        private void All_Configs_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("Configs");
        }

        private void Change_Default_Click(object sender, RoutedEventArgs e)
        {
            if (Default_Name.Text.Length != 0 && Default_Name.Text.Length < 8)
            {
                Send_Command("Change Setting, " + Config_Types.Text + ", " + Default_Name.Text);
            }
            else
            {
                MainWindow win = (MainWindow)Window.GetWindow(this);
                win.Warning_Status("Invalid value, not saved!");
                Name_Instruct.Foreground = Brushes.Red;
            }
        }

        private void Change_Duration_Click(object sender, RoutedEventArgs e)
        {
            int duration = int.Parse(Default_Duration.Text);
            if (duration < 0 || duration > 4320)
            {
                MainWindow win = (MainWindow)Window.GetWindow(this);
                win.Warning_Status("Invalid value, not saved!");
                Duration_Instruct.Foreground = Brushes.Red;
                return;
            }
            else
            {
                Send_Command("Change Setting, Default Sniff Timer, " + duration);
            }

        }

        private void Send_Command(string command)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Send_Command(command);
        }

        private void Display_Default_Click(object sender, RoutedEventArgs e)
        {

            Send_Command($"Display Config, {Config_Types.Text}");
        }

        private void Set_RTC_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            DateTime Now = DateTime.Now;
            string formatted = Now.ToString("yy, MM, dd, HH, mm, ss", System.Globalization.CultureInfo.InvariantCulture);
            //Warning_Status(formatted);
            Send_Command("Datetime, " + formatted);
        }

        private void Set_Alarm_Click(object sender, RoutedEventArgs e)
        {
            DateTime? Alarm = Date_Time_Picker.Value;
            if (Alarm.HasValue)
            {
            MainWindow win = (MainWindow)Window.GetWindow(this);
                string formatted = Alarm.Value.ToString("yy, MM, dd, HH, mm, ss", System.Globalization.CultureInfo.InvariantCulture);
                win.Warning_Status(formatted);
            }
            else
            {
                Warning_Status("No date/time selected!");
            }
        }

        private void Alarm_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Set an alarm for the next beacon sniff/uplink.\nCurrently only one can be set at a time, this will be increased in the near future.");
        }

        private void Mode_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("After the delay (if any) has elapsed, one of two modes is entered.\nIn continuous mode, the Pico sniffs continuously until the timer runs out or you end the sniff, by pressing any key.\nIn intermittent mode, it will sleep until it detects a beacon, then sleep until the next. The sniff will cease when it has been two minutes since the last beacon was recieved, or you end the sniff by pressing any key.");
        }


        private void Duration_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Enter a number between 1 and 4320 (3 days). This includes both 1 and 4320. This is how long a continuous sniff will last, unless you interrupt it.");
        }

        private void Delay_Change_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Enter a number between 0 (start immediately) and 4320 (3 days). This includes both 0 and 4320. This delay can be interrupted by pressing any key, which cancels the sniff.");
        }

        private void Config_Name_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("This can either be the word \"Default\", which will take the name of the default from the setting stored in the Pico, or the name of another saved configuration. The default configurations can be changed in \"Sniff Settings\".A default configuration is stored on first use, the others will need be created yourself in the \"Add Config\" tab.");
        }


        private void Start_Stop_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Start a sniff with the above configurations. If there is an error with a value, e.g. the name entered doesn't match a saved config name, the Pico will not start sniffing.\nThe stop command will end any sniff currently in progress, saving a metadata page, as well as saving and processing any beacons it has already recieved.");
        }

        private void Default_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Display either the default sniff configuration, or the default uplink configuration.\nUseful to check parameters prior to usage.\n");
        }

        private void All_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Display all sniff and uplink configurations.\nOften used to retrieve the name of the desired configuration prior to usage, or prior to updating the default value.\n");
        }

        private void Erase_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Erase all saved configurations from the Pico.\nThe default configuration will be rewritten after all configurations have been erased.\n");
        }

        private void Sniff_Change_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Change the name of the default sniff configuration, or the default uplink configuration.\nUse the \"Display All Configs\" button to display all saved configurations.\nTo save a new configuration, use the \"Add Configs\" tab.");
        }

        private void Duration_Change_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Change the default sniff duration.\nThe minimum value is 1 minute, the maximum is 4320 (3 days).\nThis value is different to the delay value, set before each sniff.");
        }
        private void RTC_Help_Click(object sender, RoutedEventArgs e)
        {
            Update_Help("Set the RTC using the time of this computer.\n");
        }
    }

}
