using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DeviceService.Domain;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DeviceService.WebApi;

public class Program
{
	private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

	public static IConfiguration Configuration { get; } = GetConfiguration(new ConfigurationBuilder()).Build();

	public static void Main(string[] args)
	{
		bool runAsService = !Debugger.IsAttached && !Enumerable.Contains(args, "--console");
		if (runAsService)
		{
			Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule!.FileName)!);
		}
		ApplicationControl.IsService = runAsService;
		IConfiguration configuration = Configuration;
		Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).Enrich.WithThreadId().Enrich.FromLogContext().WriteTo.Console().CreateLogger();
		try
		{
			Log.Information("------------------------------------ Starting web host -------------------------------------");
			Log.Information("ASPNETCORE_ENVIRONMENT: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
			Log.Information($"DeviceService version: {DeviceService.Domain.DeviceService.AssemblyVersion}");
			Log.Information(runAsService ? "Program.Main - Starting as a Windows Service" : "Program.Main - Starting as a console application");
			CreateHostBuilder(args, configuration, runAsService).Build().Run();
		}
		catch (Exception exception)
		{
			Log.Fatal(exception, "Program.Main - Web host terminated with error");
		}
		finally
		{
			Log.Information("------------------------------------ Stopping web host -------------------------------------");
			Log.CloseAndFlush();
		}
	}

	public static IHostBuilder CreateHostBuilder(string[] args, IConfiguration config, bool runAsService)
	{
		Uri deviceServiceUri = GetDeviceServiceUri(config);
		Log.Information($"CreateWebHostBuilder: url={deviceServiceUri.AbsoluteUri}  port={deviceServiceUri.Port}");
		return Host.CreateDefaultBuilder(args)
			.UseWindowsService()
			.UseContentRoot(Path.GetDirectoryName(Environment.ProcessPath)!)
			.ConfigureAppConfiguration((context, conf) => GetConfiguration(conf))
			.UseSerilog()
			.ConfigureServices(services =>
			{
				if (runAsService)
				{
					services.AddHostedService<DeviceServiceWebHostService>();
				}
			})
			.ConfigureWebHostDefaults(webHost => webHost
				.UseUrls(deviceServiceUri.AbsoluteUri)
				.UseSetting("Port", deviceServiceUri.Port.ToString())
				.UseStartup<Startup>());
	}

	public static Uri GetDeviceServiceUri(IConfiguration config)
	{
		return new Uri(config.GetValue<string>("AppSettings:DeviceServiceUrl")!); // throws when the setting is missing, as before
	}

	private static IConfigurationBuilder GetConfiguration(IConfigurationBuilder builder)
	{
		builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddJsonFile("Data\\DeviceServiceConfig.json", optional: true, reloadOnChange: true);
		if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") != null)
		{
			builder.AddJsonFile("appsettings." + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") + ".json", optional: true, reloadOnChange: true);
		}
		return builder;
	}
}
