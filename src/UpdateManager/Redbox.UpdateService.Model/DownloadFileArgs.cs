using System.Collections.Generic;

namespace Redbox.UpdateService.Model;

public class DownloadFileArgs
{
	public string StoreNumber { get; set; }

	public List<long> DownloadFileList { get; set; }
}
