using System.Collections.Generic;

namespace Redbox.UpdateManager.Client;

public class ClientConfigFile
{
	public long OID { get; set; }

	public string Name { get; set; }

	public string Path { get; set; }

	public long GenerationOID { get; set; }

	public List<ClientConfigurationData> ConfigurationData { get; set; }
}
