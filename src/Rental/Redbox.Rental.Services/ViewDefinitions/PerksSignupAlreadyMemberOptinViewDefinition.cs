using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "perks_signup_already_member_optin_view")]
public class PerksSignupAlreadyMemberOptinViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PerksSignupAlreadyMemberOptinViewDefinition()
	{
		base.ViewName = "perks_signup_already_member_optin_view";
		base.ActiveFlag = true;
		base.ControlName = "PerksSignUpAlreadyMemberOptInView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.ConfigurePerksSignupAlreadymemberOptinView(viewFrameInstance?.Parameter as PerksSignupAlreadyMemberOptInViewInfo);
		};
	}
}
