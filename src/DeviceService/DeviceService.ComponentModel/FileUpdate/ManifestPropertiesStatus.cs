namespace DeviceService.ComponentModel.FileUpdate;

public enum ManifestPropertiesStatus
{
	Normal,
	MissingManifestFile,
	CorruptedManifestFile,
	ExceptionWhileReading,
	ManifestHasNoUpdateFiles,
	StartDateIsInTheFuture,
	OutsideOfTimeRange,
	FileNameTooLong,
	RevisionDowngrade,
	MissingRevisionNumber
}
