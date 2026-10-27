using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "perks_signup_already_member_view")]
public class PerksSignupAlreadyMemberViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PerksSignupAlreadyMemberViewDefinition()
	{
		base.ViewName = "perks_signup_already_member_view";
		base.ActiveFlag = true;
		base.ControlName = "PerksSignUpAlreadyMemberView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.ConfigurePerksSignupAlreadymemberView(viewFrameInstance?.Parameter as SimpleInfoWithSignUpFlow);
		};
	}
}
