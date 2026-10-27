using System;

namespace Redbox.UpdateService.Model;

public class ConfigFileDataData
{
	public long OID { get; set; }

	public long ConfigFileGenerationID { get; set; }

	public long ConfigCategoryID { get; set; }

	public string ConfigCategoryName { get; set; }

	public long ConfigSettingID { get; set; }

	public string ConfigSettingName { get; set; }

	public string ConfigSettingDataType { get; set; }

	public long ConfigDefaultTypeID { get; set; }

	public string ConfigDefaultTypeName { get; set; }

	public int ConfigDefaultTypeRank { get; set; }

	public int ConfigDefaultTypeDataType { get; set; }

	public long ConfigDefaultTypeKey { get; set; }

	public string ConfigDefaultTypeValue { get; set; }

	public string Value { get; set; }

	public DateTime EffectiveTime { get; set; }

	public DateTime? ExpireTime { get; set; }
}
