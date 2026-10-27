using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace KioskClientService.API.Services.Inventory;

[JsonConverter(typeof(StringEnumConverter))]
public enum MerchandizingOrderType
{
	[Description("RebalancingExtract")]
	RebalancingExtract = 1,
	[Description("Fill")]
	Fill,
	[Description("ThinningExtract")]
	ThinningExtract
}
