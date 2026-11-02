using System;

namespace Redbox.UpdateManager.Client;

public class ConfigurationClientItem
{
	// The string properties are set by the object initializer in ConfigurationClient.GetItems.
	public string Key { get; set; } = null!;

	public string FilePath { get; set; } = null!;

	public string DefaultCategory { get; set; } = null!;

	public TimeSpan CheckTime { get; set; }

	public bool HasCanActivate { get; set; }

	public bool HasActivateAction { get; set; }
}
