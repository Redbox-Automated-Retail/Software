using System;

namespace Redbox.KioskEngine.PreferenceManager.Preferences;

public class PreferencePageAttribute : Attribute
{
	public string Name { get; set; }

	public string Path { get; set; }

	public int TreeOrder { get; set; } = int.MaxValue;
}
