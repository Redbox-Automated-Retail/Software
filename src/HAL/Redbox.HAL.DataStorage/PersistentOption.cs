using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class PersistentOption : IPersistentOption
{
	public string Key { get; internal set; }

	public string Value { get; internal set; }

	public void UpdateValue(string value)
	{
		Value = value;
	}

	internal PersistentOption()
	{
	}
}
