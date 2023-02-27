using System;
using System.Collections.Generic;
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

    public partial class View_Configs : Page
    {
        public View_Configs()
        {
            InitializeComponent();
            Loaded += Page_Loaded;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            List<string> Configs = new List<string> { "Sniff", "Uplink" };
            Config_Types.ItemsSource = Configs;
            Config_Types.SelectedItem = Configs[0];

            MainWindow win = (MainWindow)Window.GetWindow(this);
            if (win.serial.IsOpen)
            {
                Display_Default.IsEnabled = true;
                Erase_All_Btn.IsEnabled = true;
                Change_Default.IsEnabled = true;
                Change_Duration.IsEnabled = true;
            }
        }

        private void Display_All_Btn_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("Display Configs");
        }

        private void Erase_All_Btn_Click(object sender, RoutedEventArgs e)
        {
            Send_Command("Erase Configs");
        }


        private void Change_Default_Click(object sender, RoutedEventArgs e)
        {
            if (Default_Name.Text.Length != 0 && Default_Name.Text.Length < 8)
            {
                Send_Command("Change Setting, " + Config_Types.Text + ", " +  Default_Name.Text);
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


        private void Default_Help_Click(object sender, RoutedEventArgs e)
        {
            Help_Text("Display either the default sniff configuration, or the default uplink configuration.\nUseful to check parameters prior to usage.\n");
        }

        private void All_Help_Click(object sender, RoutedEventArgs e)
        {
            Help_Text("Display all sniff and uplink configurations.\nOften used to retrieve the name of the desired configuration prior to usage, or prior to updating the default value.\n");
        }

        private void Erase_Help_Click(object sender, RoutedEventArgs e)
        {
            Help_Text("Erase all saved configurations from the Pico.\nThe default configuration will be rewritten after all configurations have been erased.\n");
        }

        private void Sniff_Change_Help_Click(object sender, RoutedEventArgs e)
        {
            Help_Text("Change the name of the default sniff configuration, or the default uplink configuration.\nUse the \"Display All Configs\" button to display all saved configurations.\nTo save a new configuration, use the \"Add Configs\" tab.");
        }

        private void Duration_Change_Help_Click(object sender, RoutedEventArgs e)
        {
            Help_Text("Change the default sniff duration.\nThe minimum value is 1 minute, the maximum is 4320 (3 days).\nThis value is different to the delay value, set before each sniff.");
        }
        
        private void Help_Text(string help_text)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Help_Text_Update(help_text);
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
    }
}
