using Microsoft.Extensions.Logging;
using SoundOutputMultiplexer.Service.Audio;
using SoundOutputMultiplexer.Service.Pipes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace SoundOutputMultiplex
{
    internal class ServiceHost : ServiceBase
    {
        private static Thread _serviceThread;
        private static bool _stopping;
        private static NamedPipesServer _pipeServer;
        private static Multiplex _multiplex;
        private static Action<string> _logAction;

        public ServiceHost()
        {
            ServiceName = "Sound Output Multiplexer";
        }

        protected override void OnStart(string[] args)
        {
            _logAction = (message) => EventLog.WriteEntry(message);
            Run(args);
        }

        protected override void OnStop()
        {
            Abort();
        }

        protected override void OnShutdown()
        {
            Abort();
        }

        public static void Run(string[] args)
        {
            if (_logAction == null)
            {
                _logAction = (message) => Console.WriteLine(message);
            }

            _serviceThread = new Thread(InitializeServiceThread)
            {
                Name = "Sound Output Multiplexer Service Thread",
                IsBackground = true
            };

            _serviceThread.Start();
        }

        public static void Abort()
        {
            _stopping = true;
        }

        private static void InitializeServiceThread()
        {
            _multiplex = new Multiplex();
            _pipeServer = new NamedPipesServer(_logAction, _multiplex);
            _pipeServer.InitializeAsync().GetAwaiter().GetResult();
            _multiplex.Start();

            while (!_stopping)
            {
                Task.Delay(100).GetAwaiter().GetResult();
            }
        }
    }
}
