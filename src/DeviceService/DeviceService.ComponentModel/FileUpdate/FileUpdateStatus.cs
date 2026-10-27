namespace DeviceService.ComponentModel.FileUpdate;

public enum FileUpdateStatus
{
	None,
	NotProcessed,
	Updated,
	Errored,
	FileMissing,
	FileNameTooLong
}
