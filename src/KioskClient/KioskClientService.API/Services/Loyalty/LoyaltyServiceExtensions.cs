using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.Loyalty;

public static class LoyaltyServiceExtensions
{
	public static IServiceCollection AddLoyaltyService(this IServiceCollection services)
	{
		return services.AddScoped<ILoyaltyService, LoyaltyService>();
	}
}
