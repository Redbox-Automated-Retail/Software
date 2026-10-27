using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "message_popup_view")]
public class MessagePopupDefinition : BaseViewDefinition, IViewDefinition
{
	public MessagePopupDefinition()
	{
		base.ViewName = "message_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "MessagePopupViewUserControl";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			OrchestrationService.StopIdleTimer("idle_timer");
			MessagePopupViewController.ConfigureMessagePopupView((ShowMessagePopupViewParameter)viewFrameInstance.Parameter);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			MessagePopupViewController.LeaveMessagePopupView(viewFrameInstance);
			OrchestrationService.ResumeIdleTimer("idle_timer");
		};
	}
}
