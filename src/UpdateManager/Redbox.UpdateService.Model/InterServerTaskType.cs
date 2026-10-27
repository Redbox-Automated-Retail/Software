namespace Redbox.UpdateService.Model;

public enum InterServerTaskType
{
	CreateRepositoryChangeSet,
	UpdateConfigFiles,
	ClearAllConfigFileSyncIds,
	UpdateStoreInfo,
	ClearAllStoreInfoSyncIds,
	ClearAllSyncIds,
	UpdateDownloadList,
	UpdateClientRepositoryData,
	CreateFileSetChangeSet
}
