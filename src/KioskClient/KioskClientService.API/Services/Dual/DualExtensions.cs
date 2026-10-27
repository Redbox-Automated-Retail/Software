using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.Dual;

public static class DualExtensions
{
	public static IServiceCollection AddDualService(this IServiceCollection services)
	{
		return services.AddSingleton<IDualCacheService, DualCacheService>().AddScoped<IDualService, DualService>();
	}
}
