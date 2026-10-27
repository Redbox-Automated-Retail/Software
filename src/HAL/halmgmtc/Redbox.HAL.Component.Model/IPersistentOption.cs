namespace Redbox.HAL.Component.Model;

public interface IPersistentOption
{
	string Key { get; }

	string Value { get; }

	void UpdateValue(string value);
}
