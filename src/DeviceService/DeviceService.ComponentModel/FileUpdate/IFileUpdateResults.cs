using System.Collections.Generic;

namespace DeviceService.ComponentModel.FileUpdate;

public interface IFileUpdateResults
{
	bool Success { get; set; }

	FileUpdateHighLevelStatus Status { get; set; }

	string StatusMessage { get; set; }

	List<IZipFileUpdateResult> ZipFileUpdateResultCollection { get; set; }
}
