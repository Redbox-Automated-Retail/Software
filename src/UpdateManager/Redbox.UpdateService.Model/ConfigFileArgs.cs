using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class ConfigFileArgs
{
	public string StoreNumber { get; set; }

	public List<ClientConfigFile> ClientConfigFilesData { get; set; }

	public int ClientConfigFileSyncID { get; set; }
}
