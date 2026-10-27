using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.HttpQueue;

public static class HttpQueueServiceExtensions
{
	public static IServiceCollection AddHttpQueueService(this IServiceCollection services)
	{
		return services.AddScoped<IHttpQueueService, HttpQueueInterface>().AddHostedService<HttpQueueService>().AddHostedService<HttpQueueDepthService>();
	}
}
