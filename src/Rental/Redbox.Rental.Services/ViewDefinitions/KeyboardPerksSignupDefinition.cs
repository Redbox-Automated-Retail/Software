using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "keyboard_perks_signup_view")]
public class KeyboardPerksSignupDefinition : BaseViewDefinition, IViewDefinition
{
	public KeyboardPerksSignupDefinition()
	{
		base.ViewName = "keyboard_perks_signup_view";
		base.ActiveFlag = true;
		base.ControlName = "KeyboardPerksSignupView";
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
