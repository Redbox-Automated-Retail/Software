using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.Planograms;

public static class PlanogramExtensions
{
	public static IServiceCollection AddPlanogramService(this IServiceCollection services)
	{
		return services.AddScoped<IPlanogramService, PlanogramService>();
	}

	public static IServiceCollection AddSyncPlanogramsJob(this IServiceCollection services)
	{
		return services.AddScoped<SyncPlanogramsJob>();
	}
}
