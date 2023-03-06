using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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
    /// Interaction logic for Tutorial.xaml
    /// </summary>
    public partial class Tutorial : Window
    {
        public Tutorial()
        {
            InitializeComponent();
            Loaded += Window_Loaded;
        }

        private void Tutorial_Frame_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void Com_Port_Click(object sender, RoutedEventArgs e)
        {
            Tutorial_Frame.NavigationUIVisibility = NavigationUIVisibility.Visible;
            Tutorial_Frame.Source = new Uri("Find_Port.xaml", UriKind.Relative);//seems to work, now to add buttons and navigate etc, photos
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
