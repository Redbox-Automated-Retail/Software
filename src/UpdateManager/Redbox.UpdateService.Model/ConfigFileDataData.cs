using System;

namespace Redbox.UpdateService.Model;

public class ConfigFileDataData
{
	public long OID { get; set; }

	public long ConfigFileGenerationID { get; set; }

	public long ConfigCategoryID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string ConfigCategoryName { get; set; } = null!;

	public long ConfigSettingID { get; set; }

	public string ConfigSettingName { get; set; } = null!;

	public string ConfigSettingDataType { get; set; } = null!;

	public long ConfigDefaultTypeID { get; set; }

	public string ConfigDefaultTypeName { get; set; } = null!;

	public int ConfigDefaultTypeRank { get; set; }

	public int ConfigDefaultTypeDataType { get; set; }

	public long ConfigDefaultTypeKey { get; set; }

	public string ConfigDefaultTypeValue { get; set; } = null!;

	public string Value { get; set; } = null!;

	public DateTime EffectiveTime { get; set; }

	public DateTime? ExpireTime { get; set; }
}
