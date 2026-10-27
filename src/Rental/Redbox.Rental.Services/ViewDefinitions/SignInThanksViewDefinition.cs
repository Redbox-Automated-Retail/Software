using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "sign_in_thanks_view")]
public class SignInThanksViewDefinition : BaseViewDefinition, IViewDefinition
{
	public SignInThanksViewDefinition()
	{
		base.ViewName = "sign_in_thanks_view";
		base.ActiveFlag = true;
		base.ControlName = "SignInThanksView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SignInThanksViewController.ConfigureView(viewFrameInstance?.Parameter as SignInThanksViewParameters);
		};
		base.OnLeave = delegate
		{
			SignInThanksViewController.LeaveView();
		};
	}
}
