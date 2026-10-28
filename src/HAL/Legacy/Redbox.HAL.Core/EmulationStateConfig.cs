namespace Redbox.HAL.Core;

public abstract class EmulationStateConfig
{
	public object GetPropertyByName(string name)
	{
		if (name == null)
		{
			return null;
		}
		object emulated = GetEmulated(name);
		if (emulated == null)
		{
			return StringExtensions.GetValueForPath(name, this);
		}
		return emulated;
	}

	public void SetPropertyByName(string name, object[] value)
	{
		if (name != null && value != null && !ProcessOption(name, value))
		{
			if (value.Length == 1)
			{
				StringExtensions.SetValueForPath(name, this, value[0]);
			}
			else
			{
				StringExtensions.SetValueForPath(name, this, value);
			}
		}
	}

	protected abstract object GetEmulated(string name);

	protected abstract bool ProcessOption(string op, object[] value);
}
