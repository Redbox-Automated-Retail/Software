namespace Redbox.Rental.Model;

public interface IMaintenanceModeReason
{
	MaintenanceModeSource Source { get; set; }

	string Code { get; set; }
}
