namespace Redbox.Rental.Model.KioskClientService.Installer;

public class PendingKiosk
{
	public int Id { get; set; }

	public string Address { get; set; }

	public string City { get; set; }

	public string State { get; set; }

	public string ZipCode { get; set; }

	public string DueTime { get; set; }

	public string MarketName { get; set; }

	public string KaseyaMarketName { get; set; }
}
