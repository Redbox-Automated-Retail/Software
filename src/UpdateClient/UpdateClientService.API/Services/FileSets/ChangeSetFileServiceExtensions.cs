using Microsoft.Extensions.DependencyInjection;

namespace UpdateClientService.API.Services.FileSets;

public static class ChangeSetFileServiceExtensions
{
	public static IServiceCollection AddChangeSetFileService(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<IChangeSetFileService, ChangeSetFileService>();
		serviceCollection.AddScoped<IRevisionChangeSetRepository, RevisionChangeSetRepository>();
		return serviceCollection;
	}
}
