namespace Redbox.Rental.Model.KioskClientService.Campaign;

public class Control : IControl
{
	public long ControlId { get; set; }

	public ControlType ControlType { get; internal set; }

	public int DisplayDuration { get; internal set; }
}
