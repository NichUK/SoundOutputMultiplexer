using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundOutputMultiplexer.Common.Models
{
    public class DeviceList
    {
        public IList<string> InputDevices { get; set; }
        public IList<string> OutputDevices { get; set; }

        public string SelectedInputDevice { get; set; }

        public IList<OutputDeviceConfiguration> SelectedOutputDevices { get; set; } = new List<OutputDeviceConfiguration>();

        public string SelectedOutputMasterDevice { get; set; }
    }
}
