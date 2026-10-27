namespace KioskClientService.API.Services.Transaction;

public enum CardSourceTypeEnum
{
	Unknown,
	Swipe,
	EMVContact,
	QuickChip,
	MSDContactless,
	EMVContactless,
	Mobile
}
