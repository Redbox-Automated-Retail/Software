using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using Newtonsoft.Json.Converters;
using Redbox.NetCore.Logging.Extensions;
using Redbox.NetCore.Middleware.Extensions;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;
using UpdateClientService.API.App;
using UpdateClientService.API.Services;
using UpdateClientService.API.Services.Broker;
using UpdateClientService.API.Services.Configuration;
using UpdateClientService.API.Services.DataUpdate;
using UpdateClientService.API.Services.DownloadService;
using UpdateClientService.API.Services.FileCache;
using UpdateClientService.API.Services.FileSets;
using UpdateClientService.API.Services.Files;
using UpdateClientService.API.Services.IoT;
using UpdateClientService.API.Services.IoT.Certificate;
using UpdateClientService.API.Services.IoT.Certificate.Security;
using UpdateClientService.API.Services.IoT.Commands;
using UpdateClientService.API.Services.IoT.Commands.Controller;
using UpdateClientService.API.Services.IoT.Commands.KioskFiles;
using UpdateClientService.API.Services.IoT.DownloadFiles;
using UpdateClientService.API.Services.IoT.FileSets;
using UpdateClientService.API.Services.IoT.IoTCommand;
using UpdateClientService.API.Services.IoT.Security.Certificate;
using UpdateClientService.API.Services.Kernel;
using UpdateClientService.API.Services.KioskCertificate;
using UpdateClientService.API.Services.KioskEngine;
using UpdateClientService.API.Services.ProxyApi;
using UpdateClientService.API.Services.Segment;
using UpdateClientService.API.Services.Transfer;
using UpdateClientService.API.Services.Utilities;

namespace UpdateClientService.API;

public class Startup
{
	public IConfiguration Configuration { get; }

	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		// MVC 2.2 used Newtonsoft.Json and conventional routing (UseMvc); both are kept so the API's JSON and routes
		// stay as they were.
		services.AddMvc(delegate(MvcOptions options)
		{
			options.EnableEndpointRouting = false;
			options.AddLoggingFilter();
			// Nullable reference types must not make non-nullable model properties [Required]: binding stays as before nullable was turned on.
			options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
		}).AddNewtonsoftJson(delegate(MvcNewtonsoftJsonOptions options)
		{
			options.SerializerSettings.Converters.Add(new StringEnumConverter());
		});
		services.AddRouting(delegate(RouteOptions options)
		{
			options.LowercaseUrls = true;
		});
		services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
		services.AddSingleton<IStoreService, StoreService>();
		services.AddMqttService();
		services.AddSingleton<IIotCommandDispatch, IotCommandDispatch>();
		services.AddScoped<IIoTCommandService, IoTCommandService>();
		services.AddSingleton<ICertificateService, CertificateService>();
		services.AddSingleton<IIoTCertificateServiceApiClient, IoTCertificateServiceApiClient>();
		services.AddSingleton<IPersistentDataCacheService, PersistentDataCacheService>();
		services.AddScoped<IDownloadService, DownloadService>();
		services.AddScoped<ICleanupService, CleanupService>();
		services.AddScoped<IIoTProcessStatusService, IoTProcessStatusService>();
		services.AddSingleton<IActiveResponseService, ActiveResponsesService>();
		services.AddFileSetService();
		services.AddScoped<IKioskEngineService, KioskEngineService>();
		services.AddScoped<IFileService, FileService>();
		services.AddScoped<IStatusService, StatusService>();
		services.AddScoped<IIoTCommandClient, IoTCommandClient>();
		services.AddScoped<IKioskFilesService, KioskFilesService>();
		services.AddScoped<IDownloadFilesService, DownloadFilesService>();
		services.AddScoped<DownloadFilesServiceJob>();
		services.AddScoped<ITransferService, TransferService>();
		services.AddScoped<IDownloader, BitsDownloader>();
		services.AddSingleton<ISecurityService, SecurityService>();
		services.AddSingleton<IEncryptionService, EncryptionService>();
		services.AddSingleton<IHashService, HashService>();
		services.AddScoped<IKioskFileSetVersionsService, KioskFileSetVersionsService>();
		services.AddScoped<IFileSetVersionsJob, FileSetVersionsJob>();
		services.AddScoped<IFileCacheService, FileCacheService>();
		services.AddChangeSetFileService();
		services.AddScoped<IFileSetCleanup, FileSetCleanup>();
		services.AddScoped<IFileSetCleanupJob, FileSetCleanupJob>();
		services.AddScoped<IFileSetDownloader, FileSetDownloader>();
		services.AddScoped<IFileSetRevisionDownloader, FileSetRevisionDownloader>();
		services.AddScoped<IFileSetTransition, FileSetTransition>();
		services.AddScoped<IZipDownloadHelper, ZipDownloadHelper>();
		services.AddScoped<ICommandLineService, CommandLineService>();
		services.AddScoped<IWindowsServiceFunctions, WindowsServiceFunctions>();
		services.AddScoped<IKernelService, KernelService>();
		services.AddConfigurationService(Configuration);
		services.AddSegmentService();
		services.AddScoped<IDataUpdateService, DataUpdateService>();
		services.AddKioskCertificatesJob();
		services.AddBrokerService();
		services.AddProxyApi();
		services.AddScheduler();
		services.AddHttpService(trackMetrics: true, () => new HttpClientHandler
		{
			AllowAutoRedirect = true,
			UseCookies = true,
			CookieContainer = new CookieContainer()
		});
		services.AddSwaggerGenNewtonsoftSupport();
		services.AddSwaggerGen(delegate(SwaggerGenOptions c)
		{
			SwaggerGenOptionsExtensions.SwaggerDoc(c, "v1", new OpenApiInfo
			{
				Title = "Update Client API",
				Version = "v1"
			});
			string path = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
			string text = Path.Combine(AppContext.BaseDirectory, path);
			if (File.Exists(text))
			{
				c.IncludeXmlComments(text);
			}
		});
		RegisterICommand(services);
	}

	public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, IOptionsSnapshot<AppSettings> settings, IOptionsSnapshotKioskConfiguration kioskConfiguration)
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
		InitializeSharedLogger(app);
		ScheduleServices(app, env, logger, settings);
		kioskConfiguration.Log();
	}

	private void RegisterICommand(IServiceCollection services)
	{
		Type icommandType = typeof(ICommandIoTController);
		IEnumerable<Type> enumerable = from p in AppDomain.CurrentDomain.GetAssemblies().SelectMany((Assembly s) => s.GetTypes())
			where icommandType.IsAssignableFrom(p) && !p.IsInterface
			select p;
		List<ICommandIoTController> commandIoTControllers = new List<ICommandIoTController>();
		foreach (Type item in enumerable)
		{
			services.AddScoped(typeof(ICommandIoTController), item);
			commandIoTControllers.Add(item as ICommandIoTController);
		}
		services.AddScoped(typeof(List<ICommandIoTController>), typeof(List<ICommandIoTController>));
	}

	private void ScheduleServices(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, IOptionsSnapshot<AppSettings> settings)
	{
		app.ApplicationServices.UseScheduler(delegate(IScheduler scheduler)
		{
			scheduler.Schedule<MqttProxyJob>().EveryMinute().PreventOverlapping("MqttProxyJob");
			if (env.IsDevelopment())
			{
				scheduler.Schedule<IFileSetVersionsJob>().EveryFiveMinutes().PreventOverlapping("IFileSetVersionsJob");
			}
			else
			{
				scheduler.Schedule<IFileSetVersionsJob>().Cron(CronConstants.AtRandomMinuteEvery12thHour).PreventOverlapping("IFileSetVersionsJob");
			}
			if (env.IsDevelopment())
			{
				scheduler.OnWorker("DownloadFilesServiceJob").Schedule<DownloadFilesServiceJob>().EveryMinute()
					.PreventOverlapping("DownloadFilesServiceJob");
			}
			else
			{
				scheduler.OnWorker("DownloadFilesServiceJob").Schedule<DownloadFilesServiceJob>().EveryFiveMinutes()
					.PreventOverlapping("DownloadFilesServiceJob");
			}
			scheduler.OnWorker("IDownloadService").Schedule<IDownloadService>().EveryThirtySeconds()
				.PreventOverlapping("IDownloadService");
			if (env.IsDevelopment())
			{
				scheduler.OnWorker("IFileSetProcessingJob").Schedule<IFileSetProcessingJob>().EverySeconds(20)
					.PreventOverlapping("IFileSetProcessingJob");
			}
			else
			{
				scheduler.OnWorker("IFileSetProcessingJob").Schedule<IFileSetProcessingJob>().EveryMinute()
					.PreventOverlapping("IFileSetProcessingJob");
			}
			scheduler.OnWorker("ICleanupService").Schedule<ICleanupService>().DailyAtHour(6)
				.Zoned(TimeZoneInfo.Local)
				.PreventOverlapping("ICleanupService");
			scheduler.OnWorker("ICleanupService").Schedule<ICleanupService>().DailyAtHour(15)
				.Zoned(TimeZoneInfo.Local)
				.PreventOverlapping("ICleanupService");
			scheduler.OnWorker("IFileSetCleanupJob").Schedule<IFileSetCleanupJob>().Cron(CronConstants.EveryXHours(12))
				.PreventOverlapping("IFileSetCleanupJob");
			scheduler.OnWorker("IConfigurationFileMissingJob").Schedule<IConfigurationFileMissingJob>().EveryMinute()
				.PreventOverlapping("IConfigurationFileMissingJob");
			if (env.IsDevelopment())
			{
				scheduler.OnWorker("IConfigurationServiceJob").Schedule<IConfigurationServiceJob>().EveryFiveMinutes()
					.PreventOverlapping("IConfigurationServiceJob");
			}
			else if ((settings?.Value?.ConfigurationSettings?.TimerIntervalHours).GetValueOrDefault() > 0)
			{
				scheduler.Schedule<IConfigurationServiceJob>().Cron(CronConstants.AtRandomMinuteEveryXHours(settings.Value.ConfigurationSettings.TimerIntervalHours)).PreventOverlapping("IConfigurationServiceJob");
			}
			if (env.IsDevelopment())
			{
				scheduler.OnWorker("IConfigurationServiceUpdateStatusJob").Schedule<IConfigurationServiceUpdateStatusJob>().EveryFiveMinutes()
					.PreventOverlapping("IConfigurationServiceUpdateStatusJob");
			}
			else
			{
				scheduler.Schedule<IConfigurationServiceUpdateStatusJob>().Cron(CronConstants.AtRandomMinuteEveryXHours(2)).PreventOverlapping("IConfigurationServiceUpdateStatusJob");
			}
			if (env.IsDevelopment())
			{
				scheduler.OnWorker("ISegmentServiceJob").Schedule<ISegmentServiceJob>().EveryFiveMinutes()
					.PreventOverlapping("ISegmentServiceJob");
			}
			else
			{
				scheduler.Schedule<ISegmentServiceJob>().Cron(CronConstants.AtRandomMinuteEveryXHours(1)).PreventOverlapping("ISegmentServiceJob");
			}
			if (env.IsDevelopment())
			{
				scheduler.OnWorker("IKioskCertificatesJob").Schedule<IKioskCertificatesJob>().EverySeconds(20)
					.PreventOverlapping("IKioskCertificatesJob");
			}
			else
			{
				scheduler.OnWorker("IKioskCertificatesJob").Schedule<IKioskCertificatesJob>().EveryTenMinutes()
					.PreventOverlapping("IKioskCertificatesJob");
			}
			if (env.IsDevelopment())
			{
				scheduler.OnWorker("IReportFailedPingsJob").Schedule<IReportFailedPingsJob>().EveryFiveMinutes()
					.PreventOverlapping("IReportFailedPingsJob");
			}
			else
			{
				scheduler.OnWorker("IReportFailedPingsJob").Schedule<IReportFailedPingsJob>().EveryFifteenMinutes()
					.PreventOverlapping("IReportFailedPingsJob");
			}
		}).OnError(delegate(Exception exception)
		{
			logger.LogError(exception, "Coravel global error");
		});
	}

	private void InitializeSharedLogger(IApplicationBuilder app)
	{
		ILoggerFactory requiredService = app.ApplicationServices.GetRequiredService<ILoggerFactory>();
		SharedLogger.Factory = requiredService;
	}
}
