using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class DownloadFileArgs
{
	// The properties below are filled by the JSON reader or by the sender's object initializer.
	public string StoreNumber { get; set; } = null!;

	public List<long> DownloadFileList { get; set; } = null!;
}
