using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class ConfigFileData
{
	public long OID { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }

	public List<ConfigFileGenerationData> ConfigFileGenerationList { get; set; }
}
