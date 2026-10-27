using System.Collections.ObjectModel;
using System.Threading;

namespace Redbox.KioskEngine.ComponentModel;

public interface ITimerService
{
	ReadOnlyCollection<ITimer> Timers { get; }

	void Reset();

	void StopAll();

	void StartAll();

	ITimer CreateTimer(string name, int? dueTime, int? period, TimerCallback callback);

	void RemoveTimer(string name);

	ITimer GetTimer(string name);

	void Wait(int milliseconds);
}
