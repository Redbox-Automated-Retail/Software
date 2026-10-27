using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Redbox.NetCore.Middleware.Extensions;
using Serilog;
using Serilog.Core;
using Serilog.Extensions.Logging;
using UpdateClientService.API.Services.Configuration;

namespace UpdateClientService.API;

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
			ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
			if (!flag)
			{
				Log.Information("Program.Main - Starting as a console application");
				CreateWebHostBuilder(args).Build().Run();
			}
			else
			{
				Log.Information("Program.Main - Starting as a Windows Service");
				CreateWebHostBuilder(args).Build().RunAsWindowsService();
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
		return SerilogWebHostBuilderExtensions.UseSerilog(WebHost.CreateDefaultBuilder(args).UseUrls(serviceUri.AbsoluteUri).UseSetting("Port", serviceUri.Port.ToString()), (ILogger)null, false, (LoggerProviderCollection)null).ConfigureAppConfiguration(delegate(WebHostBuilderContext webHostBuilderContext, IConfigurationBuilder conf)
		{
			GetConfiguration(conf, webHostBuilderContext);
		}).UseStartup<Startup>()
			.UseAppMetrics();
	}

	private static Uri GetServiceUri(IConfiguration config)
	{
		return new Uri(config.GetValue("AppSettings:BaseServiceUrl", string.Empty));
	}

	private static IConfigurationBuilder GetConfiguration(IConfigurationBuilder builder, WebHostBuilderContext webHostBuilderContext = null)
	{
		builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddJsonFile(ConfigurationService.ConfigurationFilePath, optional: true, reloadOnChange: true);
		string text = webHostBuilderContext?.HostingEnvironment?.EnvironmentName ?? "Production";
		builder.AddJsonFile("appsettings." + text + ".json", optional: true, reloadOnChange: true);
		return builder;
	}
}
