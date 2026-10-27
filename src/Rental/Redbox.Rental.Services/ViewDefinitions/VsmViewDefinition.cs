using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "vsm_view")]
public class VsmViewDefinition : BaseViewDefinition, IViewDefinition
{
	public VsmViewDefinition()
	{
		base.ViewName = "vsm_view";
		base.ActiveFlag = true;
		base.ControlName = "VsmView";
		base.OnEnter = delegate
		{
			VsmViewController.ConfigureVsmView();
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			VsmViewController.LeaveVsmView(viewFrameInstance);
		};
	}
}
