using System;
using System.Net.Http;
using App.Metrics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Redbox.NetCore.Middleware.Abstractions;
using Redbox.NetCore.Middleware.Http;
using Redbox.NetCore.Middleware.Metrics;
using Redbox.NetCore.Middleware.Middleware;

namespace Redbox.NetCore.Middleware.Extensions;

public static class ServiceCollectionExtensions
{
	public static void AddKioskMetrics(this IServiceCollection services, IMetricsRoot metricsRoot)
	{
		services.AddMetrics();
		services.AddSingleton((IMetrics)metricsRoot);
		services.AddSingleton<IApplicationMetrics, ApplicationMetrics>();
		services.AddTransient<ClientMetricsHandler>();
		services.AddHttpClient<IClientWrapper, ClientWrapper>().AddHttpMessageHandler<ClientMetricsHandler>();
	}

	public static void ConfigureApplicationMetricsSettings(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<ApplicationMetricsSettings>(configuration.GetSection("MetricsAppSettings"));
	}

	public static void AddTestApplicationMetrics(this IServiceCollection services)
	{
		services.AddScoped((Func<IServiceProvider, IApplicationMetrics>)delegate(IServiceProvider provider)
		{
			IOptionsSnapshot<ApplicationMetricsSettings> requiredService = provider.GetRequiredService<IOptionsSnapshot<ApplicationMetricsSettings>>();
			return new ApplicationMetrics(provider.GetRequiredService<IMetrics>(), requiredService);
		});
	}

	public static void AddGrafanaMetricsSingleton(this IServiceCollection services)
	{
		services.AddSingleton<IApplicationMetrics, ApplicationMetrics>();
		services.AddSingleton<IGrafanaMetrics, GrafanaMetrics>();
	}

	public static void AddHttpService(this IServiceCollection services, bool trackMetrics = true, Func<HttpClientHandler> primaryMessageHandler = null)
	{
		services.TryAddSingleton<ICommonMetricsFactory, CommonMetricsFactory>();
		if (trackMetrics)
		{
			services.AddSingleton<HttpMetrics>().AddSingleton<ICommonMetrics, HttpMetrics>((IServiceProvider s) => s.GetService<HttpMetrics>());
		}
		if (primaryMessageHandler == null)
		{
			services.AddHttpClient<IHttpService, HttpService>();
		}
		else
		{
			services.AddHttpClient<IHttpService, HttpService>().ConfigurePrimaryHttpMessageHandler(primaryMessageHandler);
		}
	}

	public static void AddDbMetrics(this IServiceCollection services)
	{
		services.TryAddSingleton<ICommonMetricsFactory, CommonMetricsFactory>();
		services.AddSingleton<DbMetrics>().AddSingleton<ICommonMetrics, DbMetrics>((IServiceProvider s) => s.GetRequiredService<DbMetrics>());
	}

	public static IServiceCollection AddMiddlewareData(this IServiceCollection services)
	{
		return services.AddScoped<IMiddlewareData, MiddlewareData>();
	}

	public static void AddGenericMetrics(this IServiceCollection services)
	{
		services.TryAddSingleton<ICommonMetricsFactory, CommonMetricsFactory>();
		services.AddSingleton<GenericMetrics>().AddSingleton<ICommonMetrics, GenericMetrics>((IServiceProvider s) => s.GetRequiredService<GenericMetrics>());
	}
}
