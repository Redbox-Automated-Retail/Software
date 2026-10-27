namespace KioskClientService.API.Services.Campaigns;

public interface IControl
{
	long? ControlId { get; }

	ControlType ControlType { get; }

	int? DisplayDuration { get; }
}
