using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "subscription_sign_up_confirmation_view")]
public class SubscriptionSignUpConfirmationViewDefinition : BaseViewDefinition, IViewDefinition
{
	public SubscriptionSignUpConfirmationViewDefinition()
	{
		base.ViewName = "subscription_sign_up_confirmation_view";
		base.ActiveFlag = true;
		base.ControlName = "SubscriptionSignUpConfirmationView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SubscriptionSignUpConfirmationViewController.ConfigureSubscriptionSignUpConfirmationView(viewFrameInstance.Parameter as SubscriptionSignUpConfirmationViewParameters);
		};
	}
}
