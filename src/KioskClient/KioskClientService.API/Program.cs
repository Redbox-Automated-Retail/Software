using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using KioskClientService.API.Services.Configuration;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Core;

namespace KioskClientService.API;

public class Program
{
	private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

	public static IConfiguration Configuration { get; } = GetConfiguration(new ConfigurationBuilder()).Build();

	public static void Main(string[] args)
	{
		bool flag = !Debugger.IsAttached && !Enumerable.Contains(args, "--console");
		if (flag)
		{
			string fileName = Process.GetCurrentProcess().MainModule.FileName;
			string directoryName = Path.GetDirectoryName(fileName);
			Directory.SetCurrentDirectory(directoryName);
		}
		ApplicationControl.IsService = flag;
		try
		{
			Logger logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).Enrich.WithThreadId().Enrich.FromLogContext().WriteTo.Console().CreateLogger();
			Log.Logger = logger;
		}
		catch (Exception arg)
		{
			Console.WriteLine($"Encountered unhandled exception while configuring logger. Exception -> {arg}");
			return;
		}
		try
		{
			Log.Information("------------------------------------ Starting web host -------------------------------------");
			Log.Information("ASPNETCORE_ENVIRONMENT: " + Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT"));
			Log.Information($"Service version: {Assembly.GetExecutingAssembly().GetName().Version}");
			if (!flag)
			{
				Log.Information("Program.Main - Starting as a console application");
				CreateWebHostBuilder(args).Build().Run();
			}
			else
			{
				Log.Information("Program.Main - Starting as a Windows Service");
				CreateWebHostBuilder(args).Build().RunAsDeviceServiceService();
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

	public static IWebHostBuilder CreateWebHostBuilder(string[] args)
	{
		Uri serviceUri = GetServiceUri(Configuration);
		Log.Information($"CreateWebHostBuilder: url={serviceUri.AbsoluteUri}  port={serviceUri.Port}");
		return WebHost.CreateDefaultBuilder(args).UseContentRoot(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)).UseUrls(serviceUri.AbsoluteUri)
			.UseSetting("Port", serviceUri.Port.ToString())
			.UseSerilog()
			.ConfigureAppConfiguration(delegate(WebHostBuilderContext builder, IConfigurationBuilder conf)
			{
				GetConfiguration(conf, builder);
			})
			.UseStartup<Startup>();
	}

	private static Uri GetServiceUri(IConfiguration config)
	{
		return new Uri(config.GetValue("AppSettings:ServiceUrl", string.Empty));
	}

	private static IConfigurationBuilder GetConfiguration(IConfigurationBuilder builder, WebHostBuilderContext webHostBuilderContext = null)
	{
		return builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddJsonFile(KioskConfigurationConstants.ConfigurationFilePath, optional: true, reloadOnChange: true)
			.AddJsonFile("appsettings." + (webHostBuilderContext?.HostingEnvironment?.EnvironmentName ?? "Production") + ".json", optional: true, reloadOnChange: true);
	}
}
