using Hardcodet.Wpf.TaskbarNotification;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SoundOutputMultiplexer.Common.Models;
using SoundOutputMultiplexer.Tray.Pipes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SoundOutputMultiplexer.Tray
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IHost? Host { get; private set; }
        private TaskbarIcon? _notifyIcon;
        private NamedPipesClient _namedPipesClient;

        public App()
        {
            Host = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                })
                .Build();

            _namedPipesClient = Host.Services.GetRequiredService<NamedPipesClient>();
            _namedPipesClient.InitializeAsync().ContinueWith(t =>
                MessageBox.Show($"Error while connecting to pipe server: {t.Exception}"),
                TaskContinuationOptions.OnlyOnFaulted);
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await Host!.StartAsync();

            var mainWindow = Host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);

            _notifyIcon = (TaskbarIcon)FindResource("NotifyIcon");
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            _notifyIcon?.Dispose();
            await Host!.StopAsync();
            base.OnExit(e);
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Add Services
            services.AddSingleton<NamedPipesClient>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<DeviceList>();
        }
    }
}
