using Microsoft.Extensions.DependencyInjection;

namespace UpdateClientService.API.Services.Broker;

public static class BrokerServiceExtensions
{
	public static IServiceCollection AddBrokerService(this IServiceCollection services)
	{
		return services.AddScoped<IBrokerService, BrokerService>().AddScoped<IPingStatisticsService, PingStatisticsService>().AddScoped<IReportFailedPingsJob, ReportFailedPingsJob>();
	}
}
