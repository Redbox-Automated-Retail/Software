using System;

namespace Redbox.UpdateManager.Client;

public class ClientConfigurationData
{
	public long CategoryID { get; set; }

	// The string properties are set by the JSON reader.
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
