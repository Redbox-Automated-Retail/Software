using System.Collections.Generic;

namespace DeviceService.ComponentModel.FileUpdate;

public interface IZipFileUpdateResult
{
	string FileName { get; set; }

	ZipFileUpdateStatus Status { get; set; }

	string StatusMessage { get; set; }

	IUpdateFileManifestProperties UpdateFileManifestProperties { get; set; }

	List<IFileUpdateResult> FileUpdateResultCollection { get; set; }
}
