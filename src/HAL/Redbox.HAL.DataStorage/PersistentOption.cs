using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataStorage;

internal sealed class PersistentOption : IPersistentOption
{
	// Key and Value are set by the object initializer in SecretOptionsTable or TableTypeFactory.
	public string Key { get; internal set; } = null!;

	public string Value { get; internal set; } = null!;

	public void UpdateValue(string value)
	{
		Value = value;
	}

	internal PersistentOption()
	{
	}
}
