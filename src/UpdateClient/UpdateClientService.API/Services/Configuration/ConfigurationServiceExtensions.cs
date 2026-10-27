using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace UpdateClientService.API.Services.Configuration;

public static class ConfigurationServiceExtensions
{
	public static IServiceCollection AddConfigurationService(this IServiceCollection serviceCollection, IConfiguration configuration)
	{
		serviceCollection.Configure<KioskConfigurationSettings>(configuration.GetSection("KioskConfigurationSettings"));
		serviceCollection.AddSingleton<IOptionsKioskConfiguration, OptionsKioskConfiguration>();
		serviceCollection.AddScoped<IOptionsSnapshotKioskConfiguration, OptionsSnapshotKioskConfiguration>();
		serviceCollection.AddSingleton<IOptionsMonitorKioskConfiguration, OptionsMonitorKioskConfiguration>();
		serviceCollection.AddScoped<IConfigurationService, ConfigurationService>();
		serviceCollection.AddScoped<IConfigurationServiceJob, ConfigurationServiceJob>();
		serviceCollection.AddScoped<IConfigurationFileMissingJob, ConfigurationFileMissingJob>();
		serviceCollection.AddScoped<IConfigurationServiceUpdateStatusJob, ConfigurationServiceUpdateStatusJob>();
		return serviceCollection;
	}
}
