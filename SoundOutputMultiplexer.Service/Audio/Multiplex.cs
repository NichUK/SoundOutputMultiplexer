using CSCore.CoreAudioAPI;
using CSCore.DSP;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using Microsoft.Win32;
using MoreLinq;
using SoundOutputMultiplexer.Common.Interfaces;
using SoundOutputMultiplexer.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoundOutputMultiplexer.Service.Audio
{
    public class Multiplex : ISoundOutputMultiplexerServer, IDisposable
    {
        // Registry settings
        const string UserRoot = "HKEY_CURRENT_USER";
        const string Subkey = "SeerstoneSoftware\\SoundOutputMultiplexer";
        const string RegistryPath = UserRoot + "\\" + Subkey;
        const string ItemSelectedInputDevice = "SelectedInputDevice";
        const string ItemSelectedOutputDevices = "SelectedOutputDevices";
        const string ItemSelectedOutputMasterDevice = "SelectedOutputMasterDevice";

        private string _selectedInputDeviceName;
        private string _selectedOutputMasterDeviceName;
        private IList<OutputDeviceConfiguration> _selectedOutputDeviceConfigurations;
        private float _masterVolume;
        private bool _isStarted = false;

        private MMDevice _inputDevice;
        private MMDevice _outputMasterDevice;
        private IList<MMDevice> _outputDevices;
        private IList<WasapiCapture> _inputSources;
        private IList<WasapiOut> _outputs;
        private Timer _volTimer;
        private IList<DmoChannelResampler> _dsps;

        public bool IsStarted => _isStarted;

        public Multiplex()
        {
            _selectedInputDeviceName = GetInputDeviceFromRegistry();
            _selectedOutputDeviceConfigurations = GetOutputDevicesFromRegistry();
            _selectedOutputMasterDeviceName = GetOutputMasterDeviceFromRegistry();
            _outputDevices = new List<MMDevice>();
            _inputSources = new List<WasapiCapture>();
            _outputs = new List<WasapiOut>();
            _dsps = new List<DmoChannelResampler>();
        }

        public bool Start()
        {
            if (string.IsNullOrEmpty(_selectedInputDeviceName)
                || _selectedOutputDeviceConfigurations.Count == 0
                || string.IsNullOrEmpty(_selectedOutputMasterDeviceName))
            {
                // we have no configuration, so we can't start the engine
                return false;
            }

            _inputDevice = GetDevice(_selectedInputDeviceName, DataFlow.Capture);
            _outputMasterDevice = GetDevice(_selectedOutputMasterDeviceName, DataFlow.Render);
            var inputVolume = AudioEndpointVolume.FromDevice(_outputMasterDevice);

            foreach (var deviceConfiguration in _selectedOutputDeviceConfigurations)
            {
                var outputDevice = GetDevice(deviceConfiguration.DeviceName, DataFlow.Render);
                _outputDevices.Add(outputDevice);
                var input = new WasapiCapture(false, AudioClientShareMode.Shared, 30);
                _inputSources.Add(input);
                input.Device = _inputDevice;
                input.Initialize();
                var source = new SoundInSource(input) { FillWithZeros = true };

                var panMatrix = ChannelMatrix.StereoToMonoMatrix;
                panMatrix[0, 0].Value = deviceConfiguration.Pan < 0 
                    ? -deviceConfiguration.Pan * deviceConfiguration.Volume : 0; // left channel
                panMatrix[1, 0].Value = deviceConfiguration.Pan > 0 
                    ? deviceConfiguration.Pan * deviceConfiguration.Volume : 0; // right channel
                var dspSource = new DmoChannelResampler(source, panMatrix);

                //input.Start();
                var output = new WasapiOut();
                _outputs.Add(output);

                output.Device = outputDevice;
                output.Initialize(dspSource);
                output.Volume = _masterVolume;
                //output.Play();
            }

            _inputSources.ForEach(i => i.Start());
            _outputs.ForEach(i => i.Play());

            _volTimer = new Timer((state) =>
            {
                var masterVolume = inputVolume.GetMasterVolumeLevelScalar();
                if (masterVolume != _masterVolume)
                {
                    _masterVolume = masterVolume;
                    _outputs.ForEach(o => o.Volume = _masterVolume);
                    Console.WriteLine($"Volume: {_masterVolume}");
                }
            }, null, 0, 500);

            _isStarted = true;
            return true;
        }

        public void Stop()
        {
            _isStarted = false;
            _volTimer.Dispose();
            _outputs.ForEach(o => o.Stop());

            _inputDevice.Dispose();
            _outputMasterDevice.Dispose();
            _outputs.ForEach(_o => _o.Dispose());
            _outputs.Clear();
            _outputDevices.ForEach(od => od.Dispose());
            _outputDevices.Clear();
            _inputSources.ForEach(i => i.Dispose());
            _inputSources.Clear();
        }

        public PipeMessage ReceiveEnumerateDevicesMessage()
        {
            return new PipeMessage()
            {
                Action = PipeMessage.ActionType.DeviceList,
                MessageData = System.Text.Json.JsonSerializer.Serialize(new DeviceList()
                {
                    InputDevices = GetDevices(DataFlow.Capture),
                    OutputDevices = GetDevices(DataFlow.Render),
                    SelectedInputDevice = _selectedInputDeviceName,
                    SelectedOutputDevices = _selectedOutputDeviceConfigurations,
                    SelectedOutputMasterDevice = _selectedOutputMasterDeviceName
                })
            };
        }

        public MMDevice GetDevice(string friendlyName, DataFlow dataFlow)
        {
            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                using (var deviceCollection = deviceEnumerator.EnumAudioEndpoints(dataFlow, DeviceState.Active))
                {
                    foreach (var device in deviceCollection)
                    {
                        if (device.FriendlyName == friendlyName)
                        {
                            return device;
                        }
                    }
                }
            }

            return null;
        }

        public IList<string> GetDevices(DataFlow dataFlow)
        {
            var output = new List<string>();

            using (var deviceEnumerator = new MMDeviceEnumerator())
            {
                using (var deviceCollection = deviceEnumerator.EnumAudioEndpoints(dataFlow, DeviceState.Active))
                {
                    foreach (var device in deviceCollection)
                    {
                        output.Add(device.FriendlyName);
                    }
                }
            }

            return output;
        }

        public void SetInputDevice(string? name)
        {
            if (string.IsNullOrEmpty(name)) return;
            _selectedInputDeviceName = name;
            SetInputDeviceInRegistry(_selectedInputDeviceName);
        }

        public void SetOutputDevices(IList<OutputDeviceConfiguration>? outputDevices)
        {
            if (outputDevices == null) return;
            _selectedOutputDeviceConfigurations = outputDevices;
            SetOutputDevicesInRegistry(_selectedOutputDeviceConfigurations);
        }

        public void SetOutputMasterDevice(string? name)
        {
            if (string.IsNullOrEmpty(name)) return;
            _selectedOutputMasterDeviceName = name;
            SetOutputMasterDeviceInRegistry(_selectedOutputMasterDeviceName);
        }


        private string GetInputDeviceFromRegistry()
        {
            return (string)(Registry.GetValue(RegistryPath, ItemSelectedInputDevice, string.Empty) ?? String.Empty);
        }

        private IList<OutputDeviceConfiguration> GetOutputDevicesFromRegistry()
        {
            var deviceConfigurations = (string[])
                (Registry.GetValue(RegistryPath, ItemSelectedOutputDevices, new string[0]) ?? new string[0]);

            return deviceConfigurations.Select(d => OutputDeviceConfiguration.FromString(d)).ToList();
        }

        private string GetOutputMasterDeviceFromRegistry()
        {
            return (string)(Registry.GetValue(RegistryPath, ItemSelectedOutputMasterDevice, string.Empty) ?? String.Empty);
        }

        private void SetInputDeviceInRegistry(string deviceName)
        {
            Registry.SetValue(RegistryPath, ItemSelectedInputDevice, deviceName);
        }

        private void SetOutputDevicesInRegistry(IList<OutputDeviceConfiguration> outputDevices)
        {
            var deviceConfigurationStrings = outputDevices.Select(d => d.ToString()).ToArray();
            Registry.SetValue(RegistryPath, ItemSelectedOutputDevices, deviceConfigurationStrings);
        }

        private void SetOutputMasterDeviceInRegistry(string deviceName)
        {
            Registry.SetValue(RegistryPath, ItemSelectedOutputMasterDevice, deviceName);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
