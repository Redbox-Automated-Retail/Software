using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.Pricing;

public static class PricingExtensions
{
	public static IServiceCollection AddPricingService(this IServiceCollection services)
	{
		return services.AddScoped<IPricingService, PricingService>();
	}

	public static IServiceCollection AddPricingJob(this IServiceCollection services)
	{
		return services.AddScoped<PricingJob>();
	}
}
