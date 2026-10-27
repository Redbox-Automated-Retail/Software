namespace DeviceService.ComponentModel.FileUpdate;

public enum FileUpdateHighLevelStatus
{
	Normal,
	UnableToShutDown,
	UnableToCreateCompletedFolder,
	MissingFileUpdater,
	NearExpectedRebootTime,
	UnableToCreateNotProcessedFolder
}
