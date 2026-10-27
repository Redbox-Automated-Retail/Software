using System;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal sealed class ConfigurationCallbackEntry : ICallbackEntry
{
	public string Name => "ConfigurationCallbackEntry";

	public string Store { get; set; }

	public string Path { get; set; }

	public string Key { get; set; }

	public string Action { get; set; }

	public string NewValue { get; set; }

	public ConfigurationCallback Function { get; set; }

	public void Invoke()
	{
		if (Function == null)
		{
			return;
		}
		try
		{
			Function(Store, Action, Path, Key, NewValue);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ConfigurationCallbackEntry handler.", e);
		}
	}
}
