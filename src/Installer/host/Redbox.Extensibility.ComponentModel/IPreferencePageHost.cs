namespace Redbox.Extensibility.ComponentModel;

public interface IPreferencePageHost
{
	bool SaveValues(IPreferencePage preferencePage);

	void LoadValues(IPreferencePage preferencePage);
}
