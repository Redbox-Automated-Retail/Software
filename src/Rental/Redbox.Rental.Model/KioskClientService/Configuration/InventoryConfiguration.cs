namespace Redbox.Rental.Model.KioskClientService.Configuration;

[Category(Name = "Inventory")]
public class InventoryConfiguration : BaseCategorySetting
{
	public int KioskTitlesCountTimeout { get; set; } = 6000;
}
