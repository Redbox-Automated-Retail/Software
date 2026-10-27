using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "keyboard_redbox_plus_signup_view")]
public class KeyboardRedboxPlusSignupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public KeyboardRedboxPlusSignupViewDefinition()
	{
		base.ViewName = "keyboard_redbox_plus_signup_view";
		base.ActiveFlag = true;
		base.ControlName = "KeyboardRedboxPlusSignupView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			KeyboardViewController.ConfigureView(viewFrameInstance?.Parameter);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			KeyboardViewController.ClosingView(viewFrameInstance?.Parameter);
		};
	}
}
