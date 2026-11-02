using System.Collections.Generic;

namespace Redbox.UpdateManager.Client;

public class ClientConfigFile
{
	public long OID { get; set; }

	// Set by the JSON reader.
	public string Name { get; set; } = null!;

	public string Path { get; set; } = null!;

	public long GenerationOID { get; set; }

	public List<ClientConfigurationData> ConfigurationData { get; set; } = null!;
}
