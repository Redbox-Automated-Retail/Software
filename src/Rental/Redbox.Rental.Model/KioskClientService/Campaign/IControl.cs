namespace Redbox.Rental.Model.KioskClientService.Campaign;

public interface IControl
{
	long ControlId { get; }

	ControlType ControlType { get; }

	int DisplayDuration { get; }
}
