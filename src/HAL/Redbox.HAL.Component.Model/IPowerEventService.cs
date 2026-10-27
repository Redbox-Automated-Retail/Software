namespace Redbox.HAL.Component.Model;

public interface IPowerEventService
{
	void Add(IPowerEventObserver observer);

	void Remove(IPowerEventObserver observer);
}
