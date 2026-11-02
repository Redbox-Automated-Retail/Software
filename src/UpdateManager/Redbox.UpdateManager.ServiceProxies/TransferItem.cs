using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class TransferItem : ITransferItem
{
	// The string properties are set by the JSON reader.
	public string Name { get; set; } = null!;

	public string Path { get; set; } = null!;

	public string RemoteURL { get; set; } = null!;

	public ulong TotalTransferd { get; set; }

	public ulong TotalBytes { get; set; }
}
