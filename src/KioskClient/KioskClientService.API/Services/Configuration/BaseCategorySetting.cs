using Newtonsoft.Json;

namespace KioskClientService.API.Services.Configuration;

public class BaseCategorySetting
{
	[JsonIgnore]
	public IKioskConfiguration ParentKioskConfiguration { get; set; }
}
