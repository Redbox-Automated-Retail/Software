namespace DeviceService.ComponentModel.FileUpdate;

public interface IFileUpdateService
{
	string UpdateFolderPath { get; set; }

	string CompletedFolderPath { get; set; }

	string NotProcessedFolderPath { get; set; }

	IFileUpdater FileUpdater { get; set; }

	IFileUpdateResults UpdateFiles();
}
