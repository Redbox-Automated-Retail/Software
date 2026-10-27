namespace Redbox.UpdateManager.ComponentModel;

public interface ITransferItem
{
	string Name { get; }

	string Path { get; }

	string RemoteURL { get; }

	ulong TotalTransferd { get; set; }

	ulong TotalBytes { get; set; }
}
