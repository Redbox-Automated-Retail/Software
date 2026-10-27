using Microsoft.Extensions.DependencyInjection;

namespace UpdateClientService.API.Services.FileSets;

public static class FileSetServiceExtensions
{
	public static IServiceCollection AddFileSetService(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<IFileSetService, FileSetService>();
		serviceCollection.AddScoped<IStateFileService, StateFileService>();
		serviceCollection.AddScoped<IStateFileRepository, StateFileRepository>();
		serviceCollection.AddScoped<IFileSetProcessingJob, FileSetProcessingJob>();
		return serviceCollection;
	}
}
