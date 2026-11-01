using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using KioskClientService.API.Services.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Core;

namespace KioskClientService.API;

public class Program
{
	private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

	public static IConfiguration Configuration { get; } = GetConfiguration(new ConfigurationBuilder()).Build();

	public static void Main(string[] args)
	{
		bool runAsService = !Debugger.IsAttached && !Enumerable.Contains(args, "--console");
		if (runAsService)
		{
			string fileName = Process.GetCurrentProcess().MainModule.FileName;
			string directoryName = Path.GetDirectoryName(fileName);
			Directory.SetCurrentDirectory(directoryName);
		}
		ApplicationControl.IsService = runAsService;
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
			// Which build runs: release builds stamp "<version>+<commit>" (build/Release.targets).
			Log.Information("Service build: " + Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion);
			Log.Information(runAsService ? "Program.Main - Starting as a Windows Service" : "Program.Main - Starting as a console application");
			CreateHostBuilder(args, runAsService).Build().Run();
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

	public static IHostBuilder CreateHostBuilder(string[] args, bool runAsService)
	{
		Uri serviceUri = GetServiceUri(Configuration);
		Log.Information($"CreateWebHostBuilder: url={serviceUri.AbsoluteUri}  port={serviceUri.Port}");
		return Host.CreateDefaultBuilder(args)
			.UseWindowsService()
			.UseContentRoot(Path.GetDirectoryName(Environment.ProcessPath))
			.UseSerilog()
			.ConfigureAppConfiguration((context, conf) => GetConfiguration(conf, context.HostingEnvironment.EnvironmentName))
			.ConfigureServices(services =>
			{
				if (runAsService)
				{
					services.AddHostedService<ApplicationWebHostService>();
				}
			})
			.ConfigureWebHostDefaults(webHost => webHost
				.UseUrls(serviceUri.AbsoluteUri)
				.UseSetting("Port", serviceUri.Port.ToString())
				.UseStartup<Startup>());
	}

	private static Uri GetServiceUri(IConfiguration config)
	{
		return new Uri(config.GetValue("AppSettings:ServiceUrl", string.Empty));
	}

	private static IConfigurationBuilder GetConfiguration(IConfigurationBuilder builder, string environmentName = null)
	{
		return builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddJsonFile(KioskConfigurationConstants.ConfigurationFilePath, optional: true, reloadOnChange: true)
			.AddJsonFile("appsettings." + (environmentName ?? "Production") + ".json", optional: true, reloadOnChange: true);
	}
}
