using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.Data;

public static class DataServiceExtensions
{
	public static IServiceCollection AddDataService(this IServiceCollection services)
	{
		return services.AddScoped<IDataClient, DataClient>().AddScoped<IDataService, DataService>();
	}
}
