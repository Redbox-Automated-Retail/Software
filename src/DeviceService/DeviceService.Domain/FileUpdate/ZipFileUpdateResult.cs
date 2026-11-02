using System.Collections.Generic;
using DeviceService.ComponentModel.FileUpdate;
using Newtonsoft.Json;

namespace DeviceService.Domain.FileUpdate;

public class ZipFileUpdateResult : IZipFileUpdateResult
{
	private List<IFileUpdateResult> _fileUpdateResultCollection = new List<IFileUpdateResult>();

	// FileName and StatusMessage are set by the object initializer in FileUpdateService.FindZipFileUpdates,
	// UpdateFileManifestProperties by FileUpdateService.ReadManifestFiles right after.
	public string FileName { get; set; } = null!;

	public ZipFileUpdateStatus Status { get; set; }

	public string StatusMessage { get; set; } = null!;

	public IUpdateFileManifestProperties UpdateFileManifestProperties { get; set; } = null!;

	public List<IFileUpdateResult> FileUpdateResultCollection
	{
		get
		{
			return _fileUpdateResultCollection;
		}
		set
		{
			_fileUpdateResultCollection = value;
		}
	}

	public override string ToString()
	{
		return "ZipFileUpdateResult: " + JsonConvert.SerializeObject(this);
	}
}
