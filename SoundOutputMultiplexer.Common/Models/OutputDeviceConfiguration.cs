using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundOutputMultiplexer.Common.Models
{
    public class OutputDeviceConfiguration
    {
        public string DeviceName { get; set; }

        public float Pan { get; set; }

        public float Volume { get; set; }

        public static OutputDeviceConfiguration FromString(string configuration)
        {
            var items = configuration.Split(':');
            if (items.Length != 3)
            {
                throw new ArgumentException("OutputDeviceConfiguration should be Name:Pan:Volume");
            }

            return new OutputDeviceConfiguration()
            {
                DeviceName = items[0],
                Pan = float.Parse(items[1]),
                Volume = float.Parse(items[2])
            };
        }

        public override string ToString()
        {
            return $"{DeviceName}:{Pan}:{Volume}";
        }
    }
}
