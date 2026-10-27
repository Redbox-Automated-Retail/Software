using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KioskClientService.API.Services.ProxyApi;

[JsonConverter(typeof(StringEnumConverter))]
public enum CampaignType : byte
{
	NotSet,
	StartScreen,
	InCart
}
