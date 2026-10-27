using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KioskClientService.API.Ads;

[JsonConverter(typeof(StringEnumConverter))]
public enum AdLocation
{
	Unknown,
	Welcome,
	TitleDetail,
	VSM,
	ZipCode,
	Pickup,
	Return,
	EmailNotification,
	Vending,
	PleaseWait,
	Multiple
}
