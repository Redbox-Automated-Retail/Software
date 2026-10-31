using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Redbox.NetCore.Middleware.Extensions;
using Serilog;
using UpdateClientService.API.Services.Configuration;

namespace UpdateClientService.API;

public class Program
{
	private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

	public static IConfiguration Configuration { get; } = GetConfiguration(new ConfigurationBuilder()).Build();

	public static void Main(string[] args)
	{
		bool runAsService = !Debugger.IsAttached && !args.Contains("--console");
		if (runAsService)
		{
			Directory.SetCurrentDirectory(Path.GetDirectoryName(Environment.ProcessPath));
		}
		try
		{
			Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Configuration).Enrich.WithThreadId().Enrich.FromLogContext().WriteTo.Console().CreateLogger();
		}
		catch (Exception exception)
		{
			Console.WriteLine($"Encountered unhandled exception while configuring logger. Exception -> {exception}");
			return;
		}
		try
		{
			Log.Information("------------------------------------ Starting web host -------------------------------------");
			Log.Information("ASPNETCORE_ENVIRONMENT: " + Environment.GetEnvironmentVariable(ASPNETCORE_ENVIRONMENT));
			Log.Information($"Service version: {Assembly.GetExecutingAssembly().GetName().Version}");
			Log.Information(runAsService ? "Program.Main - Starting as a Windows Service" : "Program.Main - Starting as a console application");
			CreateHostBuilder(args).Build().Run();
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

	public static IHostBuilder CreateHostBuilder(string[] args)
	{
		Uri serviceUri = GetServiceUri(Configuration);
		Log.Information($"CreateWebHostBuilder: url={serviceUri.AbsoluteUri}  port={serviceUri.Port}");
		return Host.CreateDefaultBuilder(args)
			.UseWindowsService()
			.UseContentRoot(Directory.GetCurrentDirectory())
			.UseSerilog()
			.ConfigureAppConfiguration((context, builder) => GetConfiguration(builder, context.HostingEnvironment.EnvironmentName))
			.ConfigureWebHostDefaults(webHost => webHost
				.UseUrls(serviceUri.AbsoluteUri)
				.UseSetting("Port", serviceUri.Port.ToString())
				.UseStartup<Startup>()
				.UseAppMetrics());
	}

	private static Uri GetServiceUri(IConfiguration config)
	{
		return new Uri(config.GetValue("AppSettings:BaseServiceUrl", string.Empty));
	}

	private static IConfigurationBuilder GetConfiguration(IConfigurationBuilder builder, string environmentName = null)
	{
		builder.SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).AddJsonFile(ConfigurationService.ConfigurationFilePath, optional: true, reloadOnChange: true);
		builder.AddJsonFile("appsettings." + (environmentName ?? "Production") + ".json", optional: true, reloadOnChange: true);
		return builder;
	}
}
