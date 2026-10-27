using System.Collections.Generic;

namespace Redbox.KioskEngine.Environment;

internal class DeviceServiceHelperSimulatorModel
{
	public string CurrentCard { get; set; }

	public Dictionary<string, CardData> StoredCardData { get; set; }
}
