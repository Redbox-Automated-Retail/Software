using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "start_view")]
public class StartViewDefinition : BaseViewDefinition, IViewDefinition
{
	public StartViewDefinition()
	{
		base.ViewName = "start_view";
		base.ActiveFlag = true;
		base.IsStartOverView = true;
		base.ControlName = "StartView";
		base.Clear = true;
		base.OnEnter = delegate
		{
			StartViewController.ConfigureStartView();
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			StartViewController.LeaveStartView(viewFrameInstance);
		};
	}
}
