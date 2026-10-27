using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "general_message_view")]
public class GeneralMessageViewDefinition : BaseViewDefinition, IViewDefinition
{
	public GeneralMessageViewDefinition()
	{
		base.ViewName = "general_message_view";
		base.ActiveFlag = true;
		base.ControlName = "GeneralMessageView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			GeneralMessageViewController.ConfigureMessageView(viewFrameInstance?.Parameter as GeneralMessageViewModel);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			GeneralMessageViewController.LeaveMessageView(viewFrameInstance?.Parameter as GeneralMessageViewModel);
		};
	}
}
