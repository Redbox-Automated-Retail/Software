using Microsoft.Extensions.Options;

namespace KioskClientService.API;

public class ApplicationSettings : IApplicationSettings
{
	public AppSettings AppSettings { get; set; }

	public ApplicationSettings(IOptionsMonitor<AppSettings> settings)
	{
		AppSettings = settings.CurrentValue;
	}

	public ApplicationSettings(AppSettings settings)
	{
		AppSettings = settings;
	}
}
