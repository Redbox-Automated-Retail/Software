using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.KioskEngineApi;

public static class KioskEngineApiExtensions
{
	public static IServiceCollection AddKioskEngineApiService(this IServiceCollection services)
	{
		return services.AddScoped<IKioskEngineApiShoppingCartClient, KioskEngineApiShoppingCartClient>().AddScoped<IKioskEngineApiProfileDataClient, KioskEngineApiProfileDataClient>();
	}
}
