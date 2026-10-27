using Redbox.Rental.Model;

namespace Redbox.Rental.Services;

public class ControlPanelService : IControlPanelService
{
	public string LoggedInUser { get; set; }
}
