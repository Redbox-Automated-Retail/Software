namespace DeviceService.ComponentModel.FileUpdate;

public interface IFileUpdateResult
{
	FileUpdateStatus FileUpdateStatus { get; set; }

	IFileModel UpdateFileModel { get; set; }
}
