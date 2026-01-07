using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace FolderSync
{
    public static class Program
    {
        public static async Task<int> Main(string[] args)
        {
            CliOptions options;

            try
            {
                options = args.Length >= 4
                    ? CliOptions.Parse(args)
                    : CliOptions.FromConsole();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Input error: " + ex.Message);
                Console.WriteLine("Usage: FolderSync <source> <replica> <intervalSeconds> <logPath>");
                return 1;
            }

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(options.LogPath, rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();

            try
            {
                using IHost host = Host.CreateDefaultBuilder(args)
                    .UseSerilog()
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(options);
                        services.AddSingleton<IFileComparer, Md5FileComparer>();
                        services.AddSingleton<ISyncService, SyncService>();
                        services.AddHostedService<SyncWorker>();
                    })
                    .Build();

                await host.RunAsync();
                return 0;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}
