using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class ConfigFileArgs
{
	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string StoreNumber { get; set; } = null!;

	public List<ClientConfigFile> ClientConfigFilesData { get; set; } = null!;

	public int ClientConfigFileSyncID { get; set; }
}
