using System;
using System.Collections.Generic;
using System.Linq;
using App.Metrics;
using App.Metrics.AspNetCore;
using App.Metrics.AspNetCore.Endpoints;
using App.Metrics.AspNetCore.Tracking;
using App.Metrics.Formatters;
using App.Metrics.Formatters.Prometheus;
using App.Metrics.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Redbox.NetCore.Middleware.Extensions;

public static class WebHostBuilderExtensions
{
	public static IWebHostBuilder ConfigureAndUseKioskMetrics(this IWebHostBuilder builder, IMetricsRoot metricsRoot)
	{
		builder.ConfigureMetrics(metricsRoot).UseMetrics(delegate(MetricsWebHostOptions options)
		{
			options.EndpointOptions = delegate(MetricEndpointsOptions endpointOptions)
			{
				endpointOptions.MetricsEndpointOutputFormatter = metricsRoot.OutputMetricsFormatters.GetType<MetricsPrometheusTextOutputFormatter>();
			};
		});
		return builder;
	}

	public static IWebHostBuilder UseAppMetrics(this IWebHostBuilder builder)
	{
		builder.ConfigureServices(delegate(WebHostBuilderContext context, IServiceCollection services)
		{
			IConfiguration configuration = context.Configuration;
			int metricsPort = configuration?.GetSection("AppMetrics:MetricsEndpointPort").Get<int>() ?? 60642;
			bool metricsEnabled = configuration?.GetSection("AppMetrics:MetricsEnabled").Get<bool>() ?? false;
			List<string> ignoredRegexPatterns = configuration?.GetSection("AppMetrics:IgnoredRouteRegexPatterns").Get<List<string>>();
			ignoredRegexPatterns = ignoredRegexPatterns?.Select((string s) => s.Replace("^/", "^")).ToList() ?? new List<string>();
			IMetricsBuilder builder2 = AppMetrics.CreateDefaultBuilder().Configuration.Configure(delegate(MetricsOptions options)
			{
				options.WithGlobalTags(delegate(Dictionary<string, string> tags, EnvironmentInfo envInfo)
				{
					tags["app_version"] = envInfo.EntryAssemblyVersion;
					tags["env"] = envInfo.RunningEnvironment;
					tags["server"] = envInfo.MachineName;
				});
				options.DefaultContextLabel = "application";
				options.Enabled = metricsEnabled;
			});
			services.AddMetrics(builder2);
			services.AddMetricsEndpoints(delegate(MetricEndpointsOptions options)
			{
				options.MetricsEndpointEnabled = metricsEnabled;
				options.MetricsEndpointOutputFormatter = new MetricsPrometheusTextOutputFormatter();
			});
			services.AddMetricsTrackingMiddleware(delegate(MetricsWebTrackingOptions options)
			{
				options.IgnoredHttpStatusCodes = new int[1] { 405 };
				options.IgnoredRoutesRegexPatterns = ignoredRegexPatterns;
				options.ApdexTSeconds = 1.0;
			});
			builder.UseAppMetricsPort(services, metricsPort, delegate(MetricsEndpointsHostingOptions options)
			{
				options.AllEndpointsPort = metricsPort;
				options.MetricsEndpoint = "/metrics";
			});
		});
		builder.UseMetricsEndpoints();
		builder.UseMetricsWebTracking();
		return builder;
	}

	private static void UseAppMetricsPort(this IWebHostBuilder builder, IServiceCollection services, int metricsPort, Action<MetricsEndpointsHostingOptions> setupAction)
	{
		string setting = builder.GetSetting(WebHostDefaults.ServerUrlsKey);
		builder.UseSetting(WebHostDefaults.ServerUrlsKey, $"{setting};http://*:{metricsPort}/");
		MetricsEndpointsHostingOptions obj = new MetricsEndpointsHostingOptions();
		setupAction(obj);
		services.Configure(setupAction);
	}
}
