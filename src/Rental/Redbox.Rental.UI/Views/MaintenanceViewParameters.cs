using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views;

public class MaintenanceViewParameters : IViewAnalyticsName
{
	public string ViewAnalyticsName { get; set; }

	public string ErrorCode { get; set; }
}
