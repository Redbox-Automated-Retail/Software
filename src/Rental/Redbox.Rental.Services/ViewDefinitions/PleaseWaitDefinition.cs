using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "please_wait_view")]
public class PleaseWaitDefinition : BaseViewDefinition, IViewDefinition
{
	public PleaseWaitDefinition()
	{
		base.ViewName = "please_wait_view";
		base.ActiveFlag = true;
		base.ControlName = "PleaseWaitView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PleaseWaitController.ConfigureView(viewFrameInstance?.Parameter as PleaseWaitParameters);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			PleaseWaitController.OnLeave(viewFrameInstance);
		};
	}
}
