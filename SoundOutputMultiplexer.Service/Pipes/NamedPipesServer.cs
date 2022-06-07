using H.Pipes;
using H.Pipes.Args;
using SoundOutputMultiplexer.Common;
using SoundOutputMultiplexer.Common.Models;
using SoundOutputMultiplexer.Service.Audio;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static SoundOutputMultiplexer.Common.Models.PipeMessage;

namespace SoundOutputMultiplexer.Service.Pipes
{
    public class NamedPipesServer : IDisposable
    {
        private PipeServer<PipeMessage> _server;
        private Action<string> _logAction;
        private Multiplex _multiplex;

        public NamedPipesServer(Action<string> logAction, Multiplex multiplex)
        {
            _logAction = logAction;
            _multiplex = multiplex;
        }

        public async Task InitializeAsync()
        {
            _server = new PipeServer<PipeMessage>(PipeConstants.Name);

            _server.ClientConnected += async (o, args) => await OnClientConnectedAsync(args);
            _server.ClientDisconnected += (o, args) => OnClientDisconnected(args);
            _server.MessageReceived += async (sender, args) => await OnMessageReceivedAsync(args.Message);
            _server.ExceptionOccurred += (o, args) => OnExceptionOccurred(args.Exception);

            await _server.StartAsync();
        }

        private async Task OnClientConnectedAsync(ConnectionEventArgs<PipeMessage> args)
        {
            _logAction.Invoke($"Client {args.Connection} is now connected!");
            await args.Connection.WriteAsync(new PipeMessage
            {
                Action = ActionType.SendMessage,
                MessageText = "Hi from server"
            });
        }

        private void OnClientDisconnected(ConnectionEventArgs<PipeMessage> args)
        {
            _logAction.Invoke($"Client {args.Connection} disconnected");
        }

        private async Task OnMessageReceivedAsync(PipeMessage? message)
        {
            if (message == null)
                return;

            switch (message.Action)
            {
                case ActionType.DeviceList:
                    throw new NotImplementedException();

                case ActionType.EnumerateDevices:
                    await _server.WriteAsync(_multiplex.ReceiveEnumerateDevicesMessage());
                    break;

                case ActionType.HideTrayIcon:
                    throw new NotImplementedException();

                case ActionType.SendMessage:
                    _logAction.Invoke($"Text from client: {message.MessageText}");
                    break;

                case ActionType.SetInputDevice:
                    _multiplex.SetInputDevice(JsonSerializer.Deserialize<string>(message.MessageData));
                    break;

                case ActionType.SetOutputDevices:
                    _multiplex.SetOutputDevices(JsonSerializer.Deserialize<IList<OutputDeviceConfiguration>>(message.MessageData));
                    break;

                case ActionType.SetOutputMasterDevice: // This is for Volume
                    _multiplex.SetOutputMasterDevice(JsonSerializer.Deserialize<string>(message.MessageData));
                    break;

                case ActionType.ShowTrayIcon:
                    throw new NotImplementedException();

                default:
                    _logAction.Invoke($"Unknown Action Type: {message.Action}");
                    throw new NotImplementedException();
            }
        }

        private void OnExceptionOccurred(Exception ex)
        {
            _logAction.Invoke($"Exception occured in pipe: {ex}");
        }

        public void Dispose()
        {
            DisposeAsync().GetAwaiter().GetResult();
        }

        public async Task DisposeAsync()
        {
            if (_server != null)
                await _server.DisposeAsync();
        }
    }
}
