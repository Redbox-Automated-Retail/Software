using KioskClientService.API.App;

namespace KioskClientService.API.Services.Configuration;

public class KioskConfigurationResponse : ApiBaseResponse
{
	public IKioskConfiguration KioskConfiguration { get; set; }
}
