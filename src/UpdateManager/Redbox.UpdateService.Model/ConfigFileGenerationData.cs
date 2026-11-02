using System;
using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class ConfigFileGenerationData
{
	public long OID { get; set; }

	public long ConfigFileOID { get; set; }

	public DateTime? CreatedOn { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public List<ConfigFileDataData> ConfigFileDataList { get; set; } = null!;
}
