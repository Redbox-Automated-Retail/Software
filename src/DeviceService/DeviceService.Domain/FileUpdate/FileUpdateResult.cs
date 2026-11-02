using DeviceService.ComponentModel.FileUpdate;

namespace DeviceService.Domain.FileUpdate;

public class FileUpdateResult : IFileUpdateResult
{
	public FileUpdateStatus FileUpdateStatus { get; set; }

	public IFileModel UpdateFileModel { get; set; } = null!; // set by the object initializer in FileUpdateService.ReadManifestFiles
}
