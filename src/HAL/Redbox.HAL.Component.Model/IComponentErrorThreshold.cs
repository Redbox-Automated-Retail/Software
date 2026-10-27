namespace Redbox.HAL.Component.Model;

public interface IComponentErrorThreshold
{
	bool Exceeded { get; }

	bool Correct();

	int Increment();

	void Reset();
}
