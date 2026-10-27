namespace Redbox.UpdateService.Model;

public enum PollReplyType
{
	None,
	UpdateRepositories,
	RepositoryChangeSet,
	UpdateConfigFiles,
	ConfigFileChangeSet,
	StoreInfoChangeSet,
	UpdateStoreInfo,
	Work,
	DownloadFile,
	StoreScheduleInfo,
	StoreFileChangeSet,
	FileSetChangeSet
}
