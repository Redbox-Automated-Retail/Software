using System;

namespace Redbox.UpdateService.Model;

public class ConfigurationData
{
	public long CategoryID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string CategoryName { get; set; } = null!;

	public string SettingName { get; set; } = null!;

	public string SettingDataType { get; set; } = null!;

	public string DefaultTypeName { get; set; } = null!;

	public int DefaultTypeRank { get; set; }

	public int DefaultTypeDataType { get; set; }

	public long DefaultTypeKey { get; set; }

	public string DefaultTypeValue { get; set; } = null!;

	public string Value { get; set; } = null!;

	public DateTime EffectiveTime { get; set; }

	public DateTime? ExpireTime { get; set; }
}
