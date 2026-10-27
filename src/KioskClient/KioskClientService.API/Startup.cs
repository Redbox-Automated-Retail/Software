using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using KioskClientService.API.Ads;
using KioskClientService.API.App;
using KioskClientService.API.Models;
using KioskClientService.API.Services;
using KioskClientService.API.Services.Ads;
using KioskClientService.API.Services.Ads.AdServer;
using KioskClientService.API.Services.Analytics;
using KioskClientService.API.Services.Analytics.KioskDataService;
using KioskClientService.API.Services.Analytics.Vistar;
using KioskClientService.API.Services.App;
using KioskClientService.API.Services.Authentication;
using KioskClientService.API.Services.Campaign;
using KioskClientService.API.Services.Campaigns;
using KioskClientService.API.Services.Configuration;
using KioskClientService.API.Services.CustomerProfile;
using KioskClientService.API.Services.Data;
using KioskClientService.API.Services.DataTableService;
using KioskClientService.API.Services.Dual;
using KioskClientService.API.Services.Encryption;
using KioskClientService.API.Services.HttpQueue;
using KioskClientService.API.Services.Installer;
using KioskClientService.API.Services.Inventory;
using KioskClientService.API.Services.Inventory.API;
using KioskClientService.API.Services.Kiosk;
using KioskClientService.API.Services.KioskEngineApi;
using KioskClientService.API.Services.Loyalty;
using KioskClientService.API.Services.Marketing;
using KioskClientService.API.Services.Personalization;
using KioskClientService.API.Services.Planograms;
using KioskClientService.API.Services.Pricing;
using KioskClientService.API.Services.ProductCatalog;
using KioskClientService.API.Services.ProductGroup;
using KioskClientService.API.Services.Products.MoreLikeThis;
using KioskClientService.API.Services.Products.ProductReelsApi;
using KioskClientService.API.Services.ProfileData;
using KioskClientService.API.Services.Promotion;
using KioskClientService.API.Services.ProxyApi;
using KioskClientService.API.Services.Session;
using KioskClientService.API.Services.Subscription;
using KioskClientService.API.Services.TestData;
using KioskClientService.API.Services.Transaction;
using KioskClientService.API.Services.Transaction.API;
using KioskClientService.API.Services.UpdateClientServiceApi;
using KioskClientService.API.Services.Utilities;
using KioskClientService.API.Vistar;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace KioskClientService.API;

public class Startup
{
	public IConfiguration Configuration { get; }

	public Startup(IConfiguration configuration)
	{
		Configuration = configuration;
	}

	public void ConfigureServices(IServiceCollection services)
	{
		MvcJsonMvcBuilderExtensions.AddJsonOptions(services.AddMvc(delegate(MvcOptions options)
		{
			options.AddLoggingActionFilter();
		}).SetCompatibilityVersion(CompatibilityVersion.Version_2_2), (Action<MvcJsonOptions>)delegate(MvcJsonOptions options)
		{
			options.SerializerSettings.Converters.Add(new StringEnumConverter());
			options.SerializerSettings.Converters.Add(new ControlListConverter());
			options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
		});
		services.PostConfigure(delegate(ApiBehaviorOptions options)
		{
			Func<ActionContext, IActionResult> builtInFactory = options.InvalidModelStateResponseFactory;
			options.InvalidModelStateResponseFactory = delegate(ActionContext context)
			{
				IActionResult actionResult = builtInFactory(context);
				ILogger<Startup> requiredService = context.HttpContext.RequestServices.GetRequiredService<ILogger<Startup>>();
				try
				{
					ControllerActionDescriptor controllerActionDescriptor = (ControllerActionDescriptor)context.ActionDescriptor;
					string text = controllerActionDescriptor.ControllerName.Trim();
					string text2 = controllerActionDescriptor.ActionName.Trim();
					requiredService.LogInformation("START: " + text + "." + text2);
					string text3 = ((actionResult as BadRequestObjectResult)?.Value)?.ToJson();
					string text4 = ((!string.IsNullOrEmpty(text3)) ? text3 : string.Join("; ", from x in context.ModelState.Values.SelectMany((ModelStateEntry x) => x.Errors)
						select x.ErrorMessage));
					requiredService.LogError(text + "." + text2 + ",ValidationProblemDetails=" + text4);
					requiredService.LogInformation($"END: {text}.{text2},statuscode={HttpStatusCode.BadRequest}");
				}
				catch (Exception exception)
				{
					requiredService?.LogError(exception, "Exception in logging of badrequest");
				}
				return actionResult;
			};
		});
		services.AddSwaggerGen(delegate(SwaggerGenOptions c)
		{
			c.SwaggerDoc("v1", new Info
			{
				Title = "Kiosk Client API",
				Version = "v1"
			});
			c.CustomSchemaIds((Type x) => x.FullName);
		});
		services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
		services.AddSingleton<IApplicationSettings, ApplicationSettings>();
		services.AddKioskConfiguration(Configuration);
		services.AddScoped<IConfigurationJob, ConfigurationJob>();
		services.AddScoped<IApplicationConfiguration, ApplicationConfiguration>();
		services.AddHttpService();
		services.AddHttpQueueService();
		services.AddSingleton<IApplicationControl, ApplicationControl>();
		services.AddSingleton<IStoreService, StoreService>();
		services.AddSingleton<IAnalyticsQueue, AnalyticsQueue>();
		services.AddSingleton<IMoreLikeThisData, MoreLikeThisData>();
		services.AddSingleton<IMoreLikeThisCache, MoreLikeThisCache>();
		services.AddSingleton<IPersistentDataCacheService, PersistentDataCacheService>();
		services.AddScheduler();
		services.AddScoped<IAnalyticsService, AnalyticsService>();
		services.AddScoped<IApplicationService, ApplicationService>();
		services.AddScoped<ISessionService, SessionService>();
		services.AddScoped<IAdService, AdServerService>();
		services.AddScoped<IAdServerClient, AdServerClient>();
		services.AddScoped<IAssetService, AssetService>();
		services.AddScoped<IVistarAPIClient, VistarAPIClient>();
		services.AddScoped<IAdServerAnalyticsClient, AdServerAnalyticsClient>();
		services.AddScoped<IKioskDataServiceClient, KioskDataServiceClient>();
		services.AddScoped<IProductReelsApiClient, ProductReelsApiClient>();
		services.AddScoped<IUpdateClientServiceClient, UpdateClientServiceClient>();
		services.AddScoped<IMoreLikeThisService, MoreLikeThisService>();
		services.AddScoped<IPersonalizationService, PersonalizationService>();
		services.AddScoped<ITestDataService, TestDataService>();
		services.AddScoped<IPersonalizationService, PersonalizationService>();
		services.AddScoped<IInventoryApiClient, InventoryApiClient>();
		services.AddScoped<IInventoryService, InventoryService>();
		services.AddScoped<IQRCodeUtilitiesService, QRCodeUtilitiesService>();
		services.AddScoped<ITransactionApiClient, TransactionApiClient>();
		services.AddScoped<ITransactionService, TransactionService>();
		services.AddScoped<IProductGroupSortOrderService, ProductGroupSortOrderService>();
		services.AddScoped<ProductGroupSortOrderJob>();
		services.AddProxyApiService();
		services.AddScoped<IAuthenticationService, AuthenticationService>();
		services.AddScoped<IInstallerService, InstallerService>();
		services.AddScoped<IMarketingService, MarketingService>();
		services.AddScoped<ISubscriptionService, SubscriptionService>();
		services.AddScoped<SubscriptionJob>();
		services.AddScoped<IEncryptionService, EncryptionService>();
		services.AddPromotionService();
		services.AddKioskEngineApiService();
		services.AddScoped<ICustomerProfileService, CustomerProfileService>();
		services.AddPricingService();
		services.AddPricingJob();
		services.AddPlanogramService();
		services.AddSyncPlanogramsJob();
		services.AddCampaignService();
		services.AddLoyaltyService();
		services.AddDataService();
		services.AddMemoryCache();
		services.AddKioskService();
		services.AddDualService();
		services.AddProductCatalogService();
		services.AddProfileDataService();
		services.AddDataTableService();
	}

	public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILogger<Startup> logger, IApplicationSettings applicationSettings, IOptionsSnapshotKioskConfiguration kioskConfigiuration)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		else
		{
			app.UseHsts();
		}
		app.ConfigureExceptionMiddleware();
		app.AddCompressedRequestPayloadMiddleware();
		app.AddUpdateMerchandizingMiddleware();
		app.UseHttpsRedirection();
		app.UseSwagger();
		app.UseSwaggerUI(delegate(SwaggerUIOptions c)
		{
			c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
		});
		app.ApplicationServices.UseScheduler(delegate(IScheduler scheduler)
		{
			Random random = new Random();
			scheduler.Schedule<IConfigurationJob>().Cron("*/3 * * * *").PreventOverlapping("kiosk");
			scheduler.Schedule<ProductGroupSortOrderJob>().Cron("*/10 * * * *").PreventOverlapping("kiosk");
			scheduler.Schedule<SubscriptionJob>().Cron("*/5 * * * *").PreventOverlapping("kiosk");
			scheduler.Schedule<PricingJob>().Cron($"*/{5} * * * *").PreventOverlapping("kiosk");
			scheduler.Schedule<SyncPlanogramsJob>().Cron("*/5 * * * *").PreventOverlapping("kiosk");
			scheduler.Schedule<CampaignJob>().EveryFifteenSeconds();
			scheduler.Schedule<IProfileDataJob>().Cron("*/1 * * * *").PreventOverlapping("kiosk");
			logger.LogInformation("Scheduling ConfigurationJob at */3");
			logger.LogInformation("Scheduling ProductGroupSortOrderJob at */10");
			logger.LogInformation("Scheduling SubscriptionJob at */5");
			logger.LogInformation($"Scheduling PricingJob at */{5} + a random 0 - {5} minutes");
			logger.LogInformation("Scheduling SyncPlanogramsJob at */5");
			logger.LogInformation("Scheduling CampaignJob every 15 seconds");
			logger.LogInformation("Scheduling ProfileDataJob at */1");
		});
		app.UseMvc();
		LogAppSettings(logger, applicationSettings, kioskConfigiuration);
	}

	private void LogAppSettings(ILogger<Startup> logger, IApplicationSettings applicationSettings, IOptionsSnapshotKioskConfiguration kioskConfigiuration)
	{
		AppSettings appSettings = applicationSettings.AppSettings;
		logger.LogInformation($"Assembly Version: {GetType().Assembly.GetName().Version}");
		logger.LogInformation("AppSettings - DataFilePath: " + appSettings.DataFilePath);
		logger.LogInformation("AppSettings - ServiceUrl: " + appSettings.ServiceUrl);
		logger.LogInformation("AppSettings - UpdateManagerAddress: " + appSettings.UpdateManagerAddress);
		logger.LogInformation($"AppSettings - UpdateManagerPort: {appSettings.UpdateManagerPort}");
		ConfigLogParameters loggingParameters = null;
		if (string.IsNullOrEmpty(kioskConfigiuration.KioskClientProxyApi.ProxyApiUrl))
		{
			loggingParameters = new ConfigLogParameters
			{
				ExcludeSettings = new List<(string, string)> { ("KioskClientProxyApi", "ProxyApiUrl") }
			};
			logger.LogInformation("AppSettings - DefaultProxyServiceUrl: " + appSettings.DefaultProxyServiceUrl);
		}
		kioskConfigiuration.Log(loggingParameters);
	}
}
