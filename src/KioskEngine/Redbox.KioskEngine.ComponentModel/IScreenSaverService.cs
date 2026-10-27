namespace Redbox.KioskEngine.ComponentModel;

public interface IScreenSaverService
{
	void EnableScreenSaver();

	void DisableScreenSaver();

	bool IsScreenSaverActive();

	bool IsScreenSaverRunning();

	int GetScreenSaverTimeout();

	void SetScreenSaverTimeout(int timeout);
}
