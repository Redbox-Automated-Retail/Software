namespace Redbox.Shell.ComponentModel;

public interface IKioskEngineService
{
	ErrorList IsEngineRunning(out bool isRunning);

	ErrorList ActivateControlPanel();

	ErrorList StartEngineWithControlPanel();
}
