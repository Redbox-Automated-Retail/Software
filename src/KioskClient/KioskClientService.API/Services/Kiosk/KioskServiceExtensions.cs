using KioskClientService.API.Services.ProxyApi;
using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.Kiosk;

public static class KioskServiceExtensions
{
	public static IServiceCollection AddKioskService(this IServiceCollection services)
	{
		return services.AddProxyApiService().AddScoped<IKioskService, KioskService>();
	}
}
