using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class ClientRepositoryArgs
{
	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string StoreNumber { get; set; } = null!;

	public List<ClientRepositoryData> ClientRepositoryData { get; set; } = null!;

	public int ClientRepositoryDataSyncID { get; set; }
}
