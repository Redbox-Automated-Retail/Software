using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IIdleTimer
{
	int? Timeout { get; set; }

	void Initialize(int timeout, Action action);

	void Start();

	void Stop();

	void Reset();

	void Suspend();

	void Resume();
}
