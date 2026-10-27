namespace Redbox.Rental.Model.KioskClientService.Configuration;

[Category(Name = "Kiosk")]
public class _KioskConfiguration : BaseCategorySetting
{
	public int NearbyKiosksTimeout { get; set; } = 6000;
}
