using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface ICallbackService
{
	int QueueCount { get; }

	void Reset();

	void Flush();

	void Resume();

	void Suspend();

	bool InvokeNextCallback();

	void EnqueueCallback(ICallbackEntry callbackEntry);

	DateTime? GetEnqueuedDateTimeFromNextCallbackEntry();

	ICallbackServiceStatistics GetStatics();

	void ResetStatistics();

	void LogStatistics();

	void LogCallbackTracking();
}
