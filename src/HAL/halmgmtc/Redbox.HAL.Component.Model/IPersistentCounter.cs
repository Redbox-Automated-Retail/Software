namespace Redbox.HAL.Component.Model;

public interface IPersistentCounter
{
	string Name { get; }

	int Value { get; }

	int Increment();

	int Decrement();

	void Reset();
}
