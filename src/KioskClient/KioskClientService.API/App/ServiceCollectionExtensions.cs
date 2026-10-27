using KioskClientService.API.Services.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.App;

public static class ServiceCollectionExtensions
{
	public static void AddKioskConfiguration(this IServiceCollection services, IConfiguration configuration)
	{
		services.Configure<KioskConfigurationSettings>(configuration.GetSection("KioskConfigurationSettings"));
		services.AddSingleton<IOptionsKioskConfiguration, OptionsKioskConfiguration>();
		services.AddScoped<IOptionsSnapshotKioskConfiguration, OptionsSnapshotKioskConfiguration>();
		services.AddSingleton<IOptionsMonitorKioskConfiguration, OptionsMonitorKioskConfiguration>();
	}
}
