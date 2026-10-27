using System;

namespace Redbox.UpdateService.Model;

public class ConfigurationData
{
	public long CategoryID { get; set; }

	public string CategoryName { get; set; }

	public string SettingName { get; set; }

	public string SettingDataType { get; set; }

	public string DefaultTypeName { get; set; }

	public int DefaultTypeRank { get; set; }

	public int DefaultTypeDataType { get; set; }

	public long DefaultTypeKey { get; set; }

	public string DefaultTypeValue { get; set; }

	public string Value { get; set; }

	public DateTime EffectiveTime { get; set; }

	public DateTime? ExpireTime { get; set; }
}
