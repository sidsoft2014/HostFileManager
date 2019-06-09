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

namespace HostFileManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel _vm;
        public MainWindow()
        {
            InitializeComponent();
            _vm = Wrapper.DataContext as MainWindowViewModel;
        }

        private void BtnAddEntry_Click(object sender, RoutedEventArgs e)
        {
            _vm.AddEntry();
        }

        private void BtnWrite_Click(object sender, RoutedEventArgs e)
        {
            _vm.Write();
        }

        private void BtnRemEntry_Click(object sender, RoutedEventArgs e)
        {
            _vm.RemoveEntry();
        }
    }
}
