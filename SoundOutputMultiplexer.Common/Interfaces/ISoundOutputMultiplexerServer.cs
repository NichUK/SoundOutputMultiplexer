using SoundOutputMultiplexer.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundOutputMultiplexer.Common.Interfaces
{
    public interface ISoundOutputMultiplexerServer
    {
        void SetInputDevice(string deviceName);
        void SetOutputDevices(IList<OutputDeviceConfiguration> outputDeviceConfigurations);
        void SetOutputMasterDevice(string deviceName);
    }

    public interface ISoundOutputMultiplexerServerAsync
    {
        Task SetInputDeviceAsync(string deviceName);
        Task SetOutputDevicesAsync(IList<OutputDeviceConfiguration> outputDeviceConfigurations);
        Task SetOutputMasterDeviceAsync(string deviceName);
    }
}
