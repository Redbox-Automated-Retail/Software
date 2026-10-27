using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class TransferItem : ITransferItem
{
	public string Name { get; set; }

	public string Path { get; set; }

	public string RemoteURL { get; set; }

	public ulong TotalTransferd { get; set; }

	public ulong TotalBytes { get; set; }
}
