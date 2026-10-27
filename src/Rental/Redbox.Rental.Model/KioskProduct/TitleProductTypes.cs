namespace Redbox.Rental.Model.KioskProduct;

public static class TitleProductTypes
{
	public static TitleType[] GameFamilyTitleTypes = new TitleType[11]
	{
		TitleType.Xbox360,
		TitleType.PS2,
		TitleType.PS3,
		TitleType.PSP,
		TitleType.Wii,
		TitleType.DS,
		TitleType.PC,
		TitleType.WiiU,
		TitleType.PS4,
		TitleType.XboxOne,
		TitleType.NintendoSwitch
	};

	public static TitleType[] MovieFamilyTitleTypes = new TitleType[4]
	{
		TitleType.DVD,
		TitleType.Bluray,
		TitleType.DigitalCode,
		TitleType._4KUHD
	};
}
