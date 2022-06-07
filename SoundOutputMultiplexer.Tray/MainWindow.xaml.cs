using SoundOutputMultiplexer.Common.Models;
using SoundOutputMultiplexer.Tray.Pipes;
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

namespace SoundOutputMultiplexer.Tray
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DeviceList _deviceList;
        private NamedPipesClient _namedPipesClient;

        public MainWindow(DeviceList deviceList, NamedPipesClient namedPipesClient)
        {
            _deviceList = deviceList;
            _namedPipesClient = namedPipesClient;
            InitializeComponent();
        }
    }
}
