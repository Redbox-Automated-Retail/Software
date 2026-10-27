using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "keyboard_sign_in_view")]
public class KeyboardSignInDefinition : BaseViewDefinition, IViewDefinition
{
	public KeyboardSignInDefinition()
	{
		base.ViewName = "keyboard_sign_in_view";
		base.ActiveFlag = true;
		base.ControlName = "KeyboardSignInView";
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
