using Microsoft.Extensions.DependencyInjection;

namespace UpdateClientService.API.Services.IoT;

public static class MqttExtensions
{
	public static IServiceCollection AddMqttService(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddSingleton<IMqttProxy, MqttProxy>();
		serviceCollection.AddSingleton<IIoTStatisticsService, IoTStatisticsService>();
		serviceCollection.AddScoped<MqttProxyJob>();
		return serviceCollection;
	}
}
