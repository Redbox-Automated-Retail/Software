using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using DeviceService.Client.Core;
using DeviceService.ComponentModel;
using DeviceService.ComponentModel.Analytics;
using DeviceService.ComponentModel.Bluefin;
using DeviceService.ComponentModel.FileUpdate;
using DeviceService.ComponentModel.KDS;
using DeviceService.Domain;
using DeviceService.Domain.FileUpdate;
using DeviceService.WebApi.Bluefin;
using DeviceService.WebApi.KDS;
using DeviceService.WebApi.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RBA_SDK;
using RBA_SDK_ComponentModel;
using Serilog;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace DeviceService.WebApi;

public class Startup
{
	public IConfiguration Configuration { get; }

	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
		services.AddSwaggerGen(delegate(SwaggerGenOptions c)
		{
			c.SwaggerDoc("v1", new Info
			{
				Title = "My API",
				Version = "v1"
			});
		});
		services.Configure<ApplicationSettings>(Configuration.GetSection("AppSettings"));
		services.Configure<DeviceServiceConfig>(Configuration.GetSection("DeviceServiceConfig"));
		ServiceProvider provider = services.BuildServiceProvider();
		ApplicationSettings value = provider.GetRequiredService<IOptionsSnapshot<ApplicationSettings>>().Value;
		services.AddSingleton((IApplicationSettings)value);
		ILogger<Startup> requiredService = provider.GetRequiredService<ILogger<Startup>>();
		requiredService.LogInformation($"AppSettings - UseSimulator: {value.UseSimulator}");
		requiredService.LogInformation("AppSettings - DataFilePath: " + value.DataFilePath);
		requiredService.LogInformation("AppSettings - DeviceServiceUrl: " + value.DeviceServiceUrl);
		requiredService.LogInformation("AppSettings - DeviceServiceClientPath: " + value.DeviceServiceClientPath);
		if (value.UseSimulator)
		{
			if (File.Exists(".\\RBA_SDK_Simulator.dll"))
			{
				Assembly assembly = Assembly.LoadFrom(".\\RBA_SDK_Simulator.dll");
				if (assembly != null)
				{
					Log.Information("Using RBA_SDK_Simulator");
					services.AddMvc().AddApplicationPart(assembly).AddControllersAsServices();
					Type type = assembly.GetType("RBA_SDK_Simulator.ISimulatorDataProvider");
					Type type2 = assembly.GetType("RBA_SDK_Simulator.SimulatorDataProvider");
					Type type3 = assembly.GetType("RBA_SDK_Simulator.ICommandQueue");
					Type type4 = assembly.GetType("RBA_SDK_Simulator.CommandQueue");
					services.AddSingleton(type, type2);
					services.AddSingleton(type3, type4);
					services.AddSingleton(typeof(IRBA_API), assembly.GetType("RBA_SDK_Simulator.RBA_API_Simulator"));
				}
			}
		}
		else
		{
			services.AddSingleton<IRBA_API, RBA_API>();
		}
		services.AddSingleton((Func<IServiceProvider, IAnalyticsService>)((IServiceProvider provider2) => new AnalyticsService(provider2.GetRequiredService<IApplicationSettings>(), provider2.GetRequiredService<ILogger<AnalyticsService>>())));
		services.AddSingleton((Func<IServiceProvider, IIUC285Notifier>)((IServiceProvider provider2) => new IUC285Notifier(provider2.GetRequiredService<ILogger<IUC285Notifier>>(), provider2.GetRequiredService<IHttpService>(), provider2.GetRequiredService<IApplicationSettings>())));
		IIUC285Proxy singletonIUC285Proxy = null;
		services.AddSingleton(delegate(IServiceProvider provider2)
		{
			if (singletonIUC285Proxy == null)
			{
				singletonIUC285Proxy = new IUC285Proxy(provider2.GetRequiredService<IRBA_API>(), provider2.GetRequiredService<ILogger<IUC285Proxy>>(), provider2.GetRequiredService<IIUC285Notifier>(), provider2.GetRequiredService<IKioskDataServiceClient>(), provider2.GetRequiredService<IApplicationSettings>(), provider2.GetRequiredService<IOptionsMonitor<DeviceServiceConfig>>());
				Log.Information("Begin Connect");
				(singletonIUC285Proxy as IUC285Proxy)?.Connect();
			}
			return singletonIUC285Proxy;
		});
		services.AddSingleton((IServiceProvider serviceProvider) => singletonIUC285Proxy as IFileUpdater);
		services.AddSingleton(Log.Logger);
		services.AddSignalR();
		services.AddSingleton<IBluefinServiceClient, BluefinServiceClient>();
		services.AddSingleton<IKioskDataServiceClient, KioskDataServiceClient>();
		services.AddSingleton<IDeviceServiceClientCore, DeviceServiceClient>();
		services.AddSingleton<IActivationService, ActivationService>();
		services.AddSingleton<IDeviceStatusService, DeviceStatusService>();
		services.AddSingleton<IHttpService, HttpService>();
		IApplicationControl singletonApplicationControl = null;
		services.AddSingleton(delegate(IServiceProvider provider2)
		{
			if (singletonApplicationControl == null)
			{
				singletonApplicationControl = new ApplicationControl(provider2.GetRequiredService<IApplicationLifetime>(), provider2.GetRequiredService<ILogger<ApplicationControl>>(), provider2.GetRequiredService<IIUC285Notifier>());
			}
			return singletonApplicationControl;
		});
		services.AddSingleton<IFileUpdateService, FileUpdateService>();
		new Task(delegate
		{
			ServiceProvider provider2 = services.BuildServiceProvider();
			provider2.GetRequiredService<IDeviceStatusService>().PostDeviceStatus();
			provider2.GetRequiredService<IFileUpdateService>();
		}).Start();
	}

	public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime applicationLifetime)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		else
		{
			app.UseHsts();
		}
		app.UseHttpsRedirection();
		app.UseSwagger();
		app.UseSwaggerUI(delegate(SwaggerUIOptions c)
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
		});
		app.UseMvc();
		app.UseSignalR(delegate(HubRouteBuilder route)
		{
			route.MapHub<CardReaderHub>("/CardReaderEvents");
		});
	}
}
