using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "sign_in_view")]
public class SignInViewDefinition : BaseViewDefinition, IViewDefinition
{
	public SignInViewDefinition()
	{
		base.ViewName = "sign_in_view";
		base.ActiveFlag = true;
		base.ControlName = "SignInView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SignInViewController.ConfigureSignInView(viewFrameInstance.Parameter as SignInViewParameters);
		};
		base.OnLeave = delegate
		{
			SignInViewController.LeaveView();
		};
	}
}
