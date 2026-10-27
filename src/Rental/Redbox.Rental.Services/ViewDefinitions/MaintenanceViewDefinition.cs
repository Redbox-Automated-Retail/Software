using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "maintenance_view")]
public class MaintenanceViewDefinition : BaseViewDefinition, IViewDefinition
{
	public MaintenanceViewDefinition()
	{
		base.ViewName = "maintenance_view";
		base.ActiveFlag = true;
		base.IsMaintenanceModeView = true;
		base.ControlName = "MaintenanceView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			MaintenanceViewController.ConfigureView(viewFrameInstance?.Parameter as string);
		};
		base.OnLeave = delegate
		{
			MaintenanceViewController.LeaveMaintenanceView();
		};
	}
}
