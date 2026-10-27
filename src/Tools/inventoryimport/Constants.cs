namespace InventoryImport;

public static class Constants
{
	public const string HalInventoryKey = "SOFTWARE\\Redbox\\HAL\\State\\Live\\Decks\\{0}";

	public const string DefaultDatabasePath = "C:\\Program Files\\Redbox\\DB\\Kiosk.mdb";

	public const string DefaultConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source={0}; User Id=; Password=";

	public const string GetAllBaysQuery = "\r\n            SELECT \r\n                i.Deck, i.Slot, i.Barcode as BarCode, i.DiskType\r\n\t        FROM \r\n                Inventory i LEFT JOIN Title t on t.TitleId =  i.TitleID\r\n            ORDER \r\n                BY Deck, Slot";
}
