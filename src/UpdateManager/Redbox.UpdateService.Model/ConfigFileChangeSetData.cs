using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class ConfigFileChangeSetData
{
	public long OID { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }

	public long GenerationOID { get; set; }

	public List<ConfigurationData> ConfigurationData { get; set; }
}
