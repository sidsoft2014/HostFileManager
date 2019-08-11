using System.Windows;

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
    }
}
