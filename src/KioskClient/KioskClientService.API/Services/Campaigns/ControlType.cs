using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KioskClientService.API.Services.Campaigns;

[JsonConverter(typeof(StringEnumConverter))]
public enum ControlType
{
	NotSet,
	StartAsset,
	Carousel
}
