using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace DeviceService.ComponentModel;

public class CardStats
{
	public Guid? SessionId { get; set; }

	public Guid Id { get; set; }

	public long KioskId { get; set; }

	public string ManufacturerSerialNumber { get; set; }

	public string RBAVersion { get; set; }

	public string TgzVersion { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public CardBrandEnum CardBrand { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public CardSourceType SourceType { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public CardReadExitType ReadResult { get; set; }

	public string ErrorCode { get; set; }

	public int RevisionNumber { get; set; }

	public bool HasVasData { get; set; }

	public bool HasPayData { get; set; }

	[JsonConverter(typeof(StringEnumConverter))]
	public WalletType WalletFormat { get; set; }

	public string VasErrorCode { get; set; }
}
