namespace Redbox.UpdateService.Model;

public enum PollRequestType
{
	None,
	Repositories,
	ConfigFiles,
	StoreInfo,
	WorkResult,
	StatusMessage,
	TransferStatisticReport,
	GiveMeRepositoryChangesets,
	StoreFiles,
	FileSets
}
