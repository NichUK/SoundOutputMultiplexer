using SoundOutputMultiplexer.Common.Models;
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

namespace SoundOutputMultiplexer.Tray.Controls
{
    /// <summary>
    /// Interaction logic for OutputDeviceConfiguration.xaml
    /// </summary>
    public partial class OutputDeviceConfigurationControl : UserControl
    {
        public OutputDeviceConfigurationControl()
        {
            InitializeComponent();
        }

        public OutputDeviceConfiguration GetOutputDeviceConfiguration()
        {
            return new OutputDeviceConfiguration()
            {
                DeviceName = (string)DeviceName.Content,
                Pan = (Single)Pan.Value,
                Volume = (Single)Volume.Value
            };
        }
    }
}
