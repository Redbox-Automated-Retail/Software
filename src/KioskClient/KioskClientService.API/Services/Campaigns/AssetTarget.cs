using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KioskClientService.API.Services.Campaigns;

[JsonConverter(typeof(StringEnumConverter))]
public enum AssetTarget
{
	ProductGroupId,
	PromoCode,
	None,
	BrowseFilter
}
