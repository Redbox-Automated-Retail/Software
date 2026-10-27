using System;
using System.IO;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.FileUpdate;
using DeviceService.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RBA_SDK;
using RBA_SDK_ComponentModel;
using Serilog;
using Serilog.Core;

namespace RBAInstaller;

internal class Program
{
	private const string ASPNETCORE_ENVIRONMENT = "ASPNETCORE_ENVIRONMENT";

	private static void BeginInstall(IFileUpdateService fus)
	{
		fus.UpdateFiles();
	}

	private static IConfiguration GetConfiguration()
	{
		return new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("rbainstallersettings.json", optional: false, reloadOnChange: true).Build();
	}

	private static ServiceProvider RegisterServices(IServiceCollection services)
	{
		services.AddSingleton(Log.Logger);
		services.AddSingleton<IRBA_API, RBA_API>();
		IIUC285Proxy singletonIUC285Proxy = null;
		services.AddSingleton(delegate(IServiceProvider provider)
		{
			if (singletonIUC285Proxy == null)
			{
				singletonIUC285Proxy = new IUC285Proxy(provider.GetService<IRBA_API>(), provider.GetService<ILogger<IUC285Proxy>>(), null, null, null, provider.GetService<IOptionsMonitor<DeviceServiceConfig>>());
				Log.Information("Begin Connect");
				(singletonIUC285Proxy as IUC285Proxy)?.Connect();
			}
			return singletonIUC285Proxy;
		});
		services.AddSingleton((IServiceProvider provider) => singletonIUC285Proxy as IFileUpdater);
		return services.BuildServiceProvider();
	}

	private static void Main(string[] args)
	{
		IConfiguration configuration = GetConfiguration();
		Serilog.ILogger obj = (Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(configuration).Enrich.WithThreadId().Enrich.FromLogContext().WriteTo.Console().CreateLogger());
		((Logger)obj).Information("-------------- Begin RBA Installer --------------");
		((Logger)obj).Information($"Version: {DeviceService.Domain.DeviceService.AssemblyVersion}");
		try
		{
			CreateHostBuilder(args).RunConsoleAsync();
		}
		catch (Exception exception)
		{
			Log.Fatal(exception, "An unhandled exception occurred.");
		}
		finally
		{
			Log.Information("-------------- End RBA Installer --------------");
			Log.CloseAndFlush();
		}
	}

	private static IHostBuilder CreateHostBuilder(string[] args)
	{
		return new HostBuilder().ConfigureAppConfiguration(delegate(HostBuilderContext builderContext, IConfigurationBuilder config)
		{
			config.AddJsonFile("Data\\DeviceServiceConfig.json", optional: true, reloadOnChange: false);
		}).ConfigureServices(delegate(HostBuilderContext hostContext, IServiceCollection services)
		{
			RegisterServices(services);
			services.AddHostedService<RBAInstaller>();
		}).ConfigureLogging(delegate(HostBuilderContext hostContext, ILoggingBuilder logging)
		{
			logging.AddSerilog();
		});
	}
}
