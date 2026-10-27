using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class PersistentCounter : IPersistentCounter
{
	public string Name { get; internal set; }

	public int Value { get; internal set; }

	public int Increment()
	{
		int value = Value + 1;
		Value = value;
		return Value;
	}

	public int Decrement()
	{
		int value = Value - 1;
		Value = value;
		return Value;
	}

	public void Reset()
	{
		Value = 0;
	}

	internal PersistentCounter()
	{
	}
}
