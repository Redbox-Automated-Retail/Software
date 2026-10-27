using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DeviceService.Domain;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace DeviceService.WebApi;

public class Program
{
	private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

	public static IConfiguration Configuration { get; } = GetConfiguration(new ConfigurationBuilder()).Build();

	public static void Main(string[] args)
	{
		bool flag = !Debugger.IsAttached && !Enumerable.Contains(args, "--console");
		if (flag)
		{
			Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
		}
		ApplicationControl.IsService = flag;
		IConfiguration configuration = Configuration;
		Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).Enrich.WithThreadId().Enrich.FromLogContext().WriteTo.Console().CreateLogger();
		try
		{
			Log.Information("------------------------------------ Starting web host -------------------------------------");
			Log.Information("ASPNETCORE_ENVIRONMENT: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
			Log.Information($"DeviceService version: {DeviceService.Domain.DeviceService.AssemblyVersion}");
			if (!flag)
			{
				Log.Information("Program.Main - Starting as a console application");
				CreateWebHostBuilder(args, configuration).Build().Run();
			}
			else
			{
				Log.Information("Program.Main - Starting as a Windows Service");
				CreateWebHostBuilder(args, configuration).Build().RunAsDeviceServiceService();
			}
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

	public static IWebHostBuilder CreateWebHostBuilder(string[] args, IConfiguration config)
	{
		Uri deviceServiceUri = GetDeviceServiceUri(config);
		Log.Information($"CreateWebHostBuilder: url={deviceServiceUri.AbsoluteUri}  port={deviceServiceUri.Port}");
		return WebHost.CreateDefaultBuilder(args).UseContentRoot(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)).UseUrls(deviceServiceUri.AbsoluteUri)
			.UseSetting("Port", deviceServiceUri.Port.ToString())
			.ConfigureAppConfiguration(delegate(WebHostBuilderContext builder, IConfigurationBuilder conf)
			{
				GetConfiguration(conf);
			})
			.UseSerilog()
			.UseStartup<Startup>();
	}

	public static Uri GetDeviceServiceUri(IConfiguration config)
	{
		return new Uri(config.GetValue<string>("AppSettings:DeviceServiceUrl"));
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
