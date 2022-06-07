using H.Pipes;
using Microsoft.Extensions.DependencyInjection;
using SoundOutputMultiplexer.Common;
using SoundOutputMultiplexer.Common.Interfaces;
using SoundOutputMultiplexer.Common.Models;
using SoundOutputMultiplexer.Tray.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using static SoundOutputMultiplexer.Common.Models.PipeMessage;

namespace SoundOutputMultiplexer.Tray.Pipes
{
    public class NamedPipesClient : IAsyncDisposable, ISoundOutputMultiplexerServerAsync
    {
        private PipeClient<PipeMessage> _client;
        private DeviceList _deviceList;

        public NamedPipesClient(DeviceList deviceList)
        {
            _deviceList = deviceList;
        }

        public async Task InitializeAsync()
        {
            if (_client != null && _client.IsConnected)
                return;

            _client = new PipeClient<PipeMessage>(PipeConstants.Name);
            _client.MessageReceived += (sender, args) => OnMessageReceived(args.Message);
            //_client.Disconnected += (o, args) => MessageBox.Show("Disconnected from server");
            //_client.Connected += (o, args) => MessageBox.Show("Connected to server");
            _client.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

            await _client.ConnectAsync();

            await _client.WriteAsync(new PipeMessage
            {
                Action = ActionType.EnumerateDevices,
            });
        }

        private async void OnMessageReceived(PipeMessage message)
        {
            switch (message.Action)
            {
                case ActionType.SendMessage:
                    //MessageBox.Show(message.MessageText);
                    break;

                case ActionType.DeviceList:
                    var devices = JsonSerializer.Deserialize<DeviceList>(message.MessageData);
                    if (devices != null)
                    {
                        _deviceList.InputDevices = devices.InputDevices;
                        _deviceList.OutputDevices = devices.OutputDevices;
                        _deviceList.SelectedInputDevice = devices.SelectedInputDevice;
                        _deviceList.SelectedOutputDevices = devices.SelectedOutputDevices;
                        _deviceList.SelectedOutputMasterDevice = devices.SelectedOutputMasterDevice;
                        LoadDevicesToWindow();
                    }
                    break;

                default:
                    MessageBox.Show($"Method {message.Action} not implemented");
                    break;
            }
        }

        private void LoadDevicesToWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var mainWindow = (MainWindow)Application.Current.MainWindow;

                // Input
                mainWindow.inputDevicePanel.Children.Clear();
                foreach (var deviceName in _deviceList.InputDevices)
                {
                    var radioButton = new RadioButton()
                    {
                        Content = deviceName,
                        IsChecked = deviceName == _deviceList.SelectedInputDevice,
                        Tag = deviceName
                    };

                    // when the radio button is checked, send a message to the server with the new device
                    radioButton.Checked += async (sender, args) =>
                    {
                        await SetInputDeviceAsync((string)(sender as RadioButton).Tag);
                    };

                    radioButton.Unchecked += (sender, args) => { /* Do stuff */ };

                    mainWindow.inputDevicePanel.Children.Add(radioButton);
                }

                // Ouput
                mainWindow.outputDevicePanel.Children.Clear();
                foreach (var deviceName in _deviceList.OutputDevices)
                {
                    var selectedDevice = _deviceList.SelectedOutputDevices.FirstOrDefault(d => d.DeviceName == deviceName);
                    var outputDeviceControl = new OutputDeviceConfigurationControl() 
                    {
                        Tag = deviceName,
                    };
                    
                    outputDeviceControl.Selected.IsChecked = selectedDevice != null;
                    outputDeviceControl.MasterSelected.IsChecked = _deviceList.SelectedOutputMasterDevice == deviceName;
                    outputDeviceControl.DeviceName.Content = deviceName;
                    outputDeviceControl.Pan.Value = selectedDevice?.Pan ?? 0;
                    outputDeviceControl.Volume.Value = selectedDevice?.Volume ?? 100;

                    // when the output device is checked, send a message to the server with the new config
                    //  for ease, we send all the output devices every time.
                    outputDeviceControl.Selected.Checked += async (sender, args) =>
                    {
                        await SetOutputDevicesAsync(GetSelectedOutputDevices());
                    };

                    // when it's unchecked, send the devices without this one
                    outputDeviceControl.Selected.Unchecked += async (sender, args) => 
                    {
                        await SetOutputDevicesAsync(GetSelectedOutputDevices());
                    };

                    // when the pan changes for a selected device...
                    outputDeviceControl.Pan.ValueChanged += async (sender, args) =>
                    {
                        if (outputDeviceControl.Selected.IsChecked.GetValueOrDefault())
                        {
                            await SetOutputDevicesAsync(GetSelectedOutputDevices());
                        }
                    };

                    // when the volume changes for a selected device
                    outputDeviceControl.Volume.ValueChanged += async (sender, args) =>
                    {
                        if (outputDeviceControl.Selected.IsChecked.GetValueOrDefault())
                        {
                            await SetOutputDevicesAsync(GetSelectedOutputDevices());
                        }
                    };

                    // master output is separate
                    outputDeviceControl.MasterSelected.Checked += async (sender, args) =>
                    {
                        await SetOutputMasterDeviceAsync((string)outputDeviceControl.DeviceName.Content);
                    };

                    mainWindow.outputDevicePanel.Children.Add(outputDeviceControl);
                }


                IList<OutputDeviceConfiguration> GetSelectedOutputDevices()
                {
                    return mainWindow.outputDevicePanel.Children
                        .OfType<OutputDeviceConfigurationControl>()
                        .Where(d => d.Selected.IsChecked.GetValueOrDefault())
                        .Select(d => d.GetOutputDeviceConfiguration())
                        .ToList();
                }
            });
        }

        private void OnExceptionOccurred(Exception exception)
        {
            MessageBox.Show($"An exception occured: {exception}");
        }

        public ValueTask DisposeAsync()
        {
            if (_client != null)
                return _client.DisposeAsync();

            return default;
        }

        public async Task SetInputDeviceAsync(string deviceName)
        {
            _deviceList.SelectedInputDevice = deviceName;
            await SendMessage(ActionType.SetInputDevice, deviceName);
        }

        public async Task SetOutputDevicesAsync(IList<OutputDeviceConfiguration> outputDeviceConfigurations)
        {
            _deviceList.SelectedOutputDevices = outputDeviceConfigurations;
            await SendMessage(ActionType.SetOutputDevices, outputDeviceConfigurations);
        }

        public async Task SetOutputMasterDeviceAsync(string deviceName)
        {
            _deviceList.SelectedOutputMasterDevice = deviceName;
            await SendMessage(ActionType.SetOutputMasterDevice, deviceName);
        }

        private async Task SendMessage(ActionType actionType, object data)
        {
            await _client.WriteAsync(new PipeMessage
            {
                Action = actionType,
                MessageData = JsonSerializer.Serialize(data)
            });
        }
    }
}
