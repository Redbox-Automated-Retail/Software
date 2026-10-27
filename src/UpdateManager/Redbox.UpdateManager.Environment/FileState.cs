namespace Redbox.UpdateManager.Environment;

public enum FileState
{
	Unknown,
	Staged,
	Activated,
	ActivationFailedOnce,
	ActivationFailedTwice,
	NoDestinationDirectory,
	NoDestinationPath,
	ActivationFailedNoRetry
}
