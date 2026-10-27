using Microsoft.Extensions.DependencyInjection;

namespace KioskClientService.API.Services.ProfileData;

public static class ProfileDataServiceExtensions
{
	public static IServiceCollection AddProfileDataService(this IServiceCollection services)
	{
		return services.AddScoped<IProfileDataService, ProfileDataService>().AddScoped<IProfileDataJob, ProfileDataJob>();
	}
}
