using System.IO;

namespace UpdateClientService.API.Services.Transfer;

public class TransferItem : ITransferItem
{
	public string Name { get; set; }

	public string Path { get; set; }

	public string RemoteURL { get; set; }

	public ulong TotalTransferd { get; set; }

	public ulong TotalBytes { get; set; }

	internal TransferItem(IBackgroundCopyFile file)
	{
		file.GetLocalName(out var pVal);
		file.GetRemoteName(out var pVal2);
		file.GetProgress(out var pVal3);
		Path = System.IO.Path.GetDirectoryName(pVal);
		Name = System.IO.Path.GetFileName(pVal);
		RemoteURL = pVal2;
		TotalBytes = pVal3.BytesTotal;
		TotalTransferd = pVal3.BytesTransferred;
	}
}
