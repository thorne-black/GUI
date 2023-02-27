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
            }
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
                string sniff_method = "";
                if (Method_S.IsChecked == true) sniff_method = "True";
                else sniff_method = "False";
                win.Send_Command(sniff_cmd + ", " + Config_Name.Text + ", " + Duration.Text + ", " + Delay.Text + ", " + sniff_method );
                Stop_Sniff.IsEnabled = true;
                Start_Sniff.IsEnabled = false;
            }
        }

        private void Stop_Sniff_Click(object sender, RoutedEventArgs e)
        {
            MainWindow win = (MainWindow)Window.GetWindow(this);
            win.Send_Command("!");
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
            if (Method_C.IsChecked == false && Method_S.IsChecked == false) return true;
            return false;
        }

    }
}
