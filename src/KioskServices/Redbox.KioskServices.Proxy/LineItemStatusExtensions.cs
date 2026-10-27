using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskServices.Proxy;

public static class LineItemStatusExtensions
{
	public static string ToVendStatus(this LineItemStatus status)
	{
		string result = "NotVended";
		switch (status)
		{
		case LineItemStatus.Pending:
		case LineItemStatus.InventoryError:
			result = "EmptyOrStuck";
			break;
		case LineItemStatus.CustomerDisabled:
			result = "CustomerDisabled";
			break;
		case LineItemStatus.ItemNotTaken:
			result = "DiskNotTaken";
			break;
		case LineItemStatus.HardwareError:
			result = "HardwareError";
			break;
		case LineItemStatus.InventoryCapacity:
			result = "KioskFull";
			break;
		case LineItemStatus.Fullfilled:
			result = "Vended";
			break;
		case LineItemStatus.NotFullfilled:
		case LineItemStatus.NotVended:
			result = "NotVended";
			break;
		}
		return result;
	}
}
