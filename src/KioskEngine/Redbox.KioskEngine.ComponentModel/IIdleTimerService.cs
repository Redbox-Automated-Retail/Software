namespace Redbox.KioskEngine.ComponentModel;

public interface IIdleTimerService
{
	void Reset();

	IIdleTimer CreateIdleTimer(string name);

	void RemoveIdleTimer(string name);

	IIdleTimer GetIdleTimer(string name);
}
