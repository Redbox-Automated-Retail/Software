namespace Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;

public class MachineSettingValueResponse : ApiServiceBaseResponse<string>
{
	public string SettingPath { get; set; }

	public string SettingName { get; set; }
}
