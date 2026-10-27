using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "email_optin_view")]
public class EmailOptInViewDefinition : BaseViewDefinition, IViewDefinition
{
	public EmailOptInViewDefinition()
	{
		base.ViewName = "email_optin_view";
		base.ControlName = "EmailOptInView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			EmailOptInViewController.ConfigureView(viewFrameInstance.Parameter as Action);
		};
		base.OnLeave = delegate
		{
			EmailOptInViewController.LeaveView();
		};
	}
}
