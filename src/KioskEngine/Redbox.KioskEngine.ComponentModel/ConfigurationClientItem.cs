using System;

namespace Redbox.KioskEngine.ComponentModel;

public class ConfigurationClientItem
{
	public string Key { get; set; }

	public string FilePath { get; set; }

	public string DefaultCategory { get; set; }

	public TimeSpan CheckTime { get; set; }

	public bool HasCanActivate { get; set; }

	public bool HasActivateAction { get; set; }
}
