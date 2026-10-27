using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "idle_timeout_message_view")]
public class IdleTimeoutMessageViewDefinition : BaseViewDefinition, IViewDefinition
{
	public IdleTimeoutMessageViewDefinition()
	{
		base.ViewName = "idle_timeout_message_view";
		base.ActiveFlag = true;
		base.Clear = false;
		base.ControlName = "IdleTimeoutMessageView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			IdleTimeoutMessageViewController.ConfigureView(viewFrameInstance);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			IdleTimeoutMessageViewController.CleanupView(viewFrameInstance);
		};
	}
}
