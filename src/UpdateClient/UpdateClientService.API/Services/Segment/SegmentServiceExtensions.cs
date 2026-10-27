using Microsoft.Extensions.DependencyInjection;

namespace UpdateClientService.API.Services.Segment;

public static class SegmentServiceExtensions
{
	public static IServiceCollection AddSegmentService(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<ISegmentService, SegmentService>();
		serviceCollection.AddScoped<ISegmentServiceJob, SegmentServiceJob>();
		return serviceCollection;
	}
}
