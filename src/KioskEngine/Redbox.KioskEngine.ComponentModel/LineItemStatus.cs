namespace Redbox.KioskEngine.ComponentModel;

public enum LineItemStatus : byte
{
	Pending,
	NotFullfilled,
	Fullfilled,
	ItemNotTaken,
	HardwareError,
	InventoryError,
	InventoryCapacity,
	ProductSelected,
	NotVended,
	CustomerDisabled
}
