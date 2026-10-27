namespace Redbox.UpdateManager.FileSets;

internal enum ChangesetState
{
	Received = 0,
	Error = 1,
	DownloadingRevision = 10,
	DownloadedRevision = 11,
	DownloadingFileSet = 12,
	DownloadedFileSet = 13,
	Staging = 20,
	Staged = 21,
	ActivationDependencyCheck = 30,
	ActivationPending = 31,
	ActivationBeforeActions = 32,
	Activating = 33,
	ActivationAfterActions = 34,
	Activated = 35
}
