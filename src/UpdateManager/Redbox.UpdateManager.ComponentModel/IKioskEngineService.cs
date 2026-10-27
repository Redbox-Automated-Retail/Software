namespace Redbox.UpdateManager.ComponentModel;

public interface IKioskEngineService
{
	ErrorList ExportSessions(string path);

	ErrorList ExportQueue(string path);

	ErrorList IsEngineRunning(out bool isRunning);

	ErrorList StartEngine();

	ErrorList StartEngine(string bundleName);

	ErrorList ReloadBundle();

	ErrorList Shutdown(int timeout, int tries);

	ErrorList Shutdown();

	ErrorList ChangeBundle(string name);

	ErrorList BringToFront();

	ErrorList ExecuteScript(string path);

	ErrorList GetMemoryUsage(out long total);

	ErrorList ShowOfflineScreen();
}
