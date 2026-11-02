using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class ConfigFileChangeSetData
{
	public long OID { get; set; }

	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string Name { get; set; } = null!;

	public string Path { get; set; } = null!;

	public long GenerationOID { get; set; }

	public List<ConfigurationData> ConfigurationData { get; set; } = null!;
}
