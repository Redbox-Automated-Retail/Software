using System;

namespace Redbox.KioskEngine.PreferenceManager.Preferences;

public interface IPreferencePage
{
	string Name { get; set; }

	string Path { get; set; }

	int TreeOrder { get; set; }

	Type PreferencePageHostType { get; set; }

	object PreferencePageHost { get; set; }
}
