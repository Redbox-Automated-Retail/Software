using System.Threading.Tasks;

namespace Redbox.KioskEngine.PreferenceManager.Preferences;

public interface IPreferencePageHost
{
	Task<bool> SaveValues(IPreferencePage preferencePage);

	void LoadValues(IPreferencePage preferencePage);
}
