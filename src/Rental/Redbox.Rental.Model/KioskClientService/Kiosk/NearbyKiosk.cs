namespace Redbox.Rental.Model.KioskClientService.Kiosk;

public class NearbyKiosk
{
	public long KioskId { get; set; }

	public bool IsDual { get; set; }

	public string Label { get; set; }

	public LocationType LocationType { get; set; }

	public string Address { get; set; }

	public decimal DistanceMiles { get; set; }
}
