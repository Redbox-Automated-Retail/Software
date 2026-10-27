using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.Promotion;

public static class PromotionServiceExtensions
{
	public static IServiceCollection AddPromotionService(this IServiceCollection services)
	{
		return services.AddScoped<IPromotionService, PromotionService>();
	}
}
