namespace Redbox.HAL.Component.Model;

public interface IPersistentCounterService
{
	IPersistentCounter Find(string name);

	IPersistentCounter Find(TimeoutCounters counter);

	IPersistentCounter Increment(string name);

	IPersistentCounter Increment(TimeoutCounters counter);

	IPersistentCounter Decrement(string name);

	IPersistentCounter Decrement(TimeoutCounters counter);

	bool Reset(IPersistentCounter counter);

	bool Reset(string name);

	void AddWeeklyResettable(IPersistentCounter counter);

	void ResetWeekly();
}
