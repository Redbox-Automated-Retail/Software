namespace Redbox.Rental.Model.KioskClientService.Configuration;

[Category(Name = "Dual")]
public class DualConfiguration : BaseCategorySetting
{
	public int RefreshTimeout { get; set; } = 6000;

	public int UpdateTitlesTimeout { get; set; } = 6000;

	public int InStockInOtherTimeout { get; set; } = 6000;
}
