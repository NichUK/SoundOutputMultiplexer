// See https://aka.ms/new-console-template for more information
using CSCore.CoreAudioAPI;
using CSCore.SoundIn;
using CSCore.SoundOut;
using CSCore.Streams;
using CSCore.Streams.Effects;
using SoundOutputMultiplex;
using System.ServiceProcess;

if (!Environment.UserInteractive)
{
    using (var serviceHost = new ServiceHost())
    {
        ServiceBase.Run(serviceHost);
        serviceHost.EventLog.WriteEntry("Sound Output Multiplexer Started");
    }
}
else
{
    Console.WriteLine("User interactive mode");
    using (var serviceHost = new ServiceHost())
    {
        ServiceHost.Run(args);
        Console.WriteLine("Press ESC to stop...");
        while (Console.ReadKey(true).Key != ConsoleKey.Escape)
        {

        }

        ServiceHost.Abort();
    }
}
