using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.DataTableService;

public static class DataTableServiceExtensions
{
	public static IServiceCollection AddDataTableService(this IServiceCollection services)
	{
		return services.AddScoped<IDataTableService, DataTableService>();
	}
}
