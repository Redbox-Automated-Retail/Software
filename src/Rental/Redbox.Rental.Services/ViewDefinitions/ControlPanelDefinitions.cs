using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "control_panel_login_view")]
public class ControlPanelDefinitions : BaseViewDefinition, IViewDefinition
{
	public ControlPanelDefinitions()
	{
		base.ViewName = "control_panel_login_view";
		base.ActiveFlag = true;
		base.ControlName = "ControlPanelLoginView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			ControlPanelViewController.ConfigureLoginView(viewFrameInstance?.Parameter as LoginInfo);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			ControlPanelViewController.ClosingLoginView(viewFrameInstance?.Parameter as LoginInfo);
		};
	}
}
