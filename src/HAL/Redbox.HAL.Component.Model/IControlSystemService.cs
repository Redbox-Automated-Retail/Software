namespace Redbox.HAL.Component.Model;

public interface IControlSystemService
{
	bool IsInitialized { get; }

	void AddHandler(IControlSystemObserver observer);

	void RemoveHandler(IControlSystemObserver observer);

	bool Restart();
}
