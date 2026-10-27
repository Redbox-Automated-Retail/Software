using DeviceService.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public static class CardSourceTypeExtensions
{
	public static bool IsEmvContactlessOrQuickChip(this CardSourceType type)
	{
		if (CardSourceType.EMVContact != type)
		{
			return CardSourceType.QuickChip == type;
		}
		return true;
	}
}
