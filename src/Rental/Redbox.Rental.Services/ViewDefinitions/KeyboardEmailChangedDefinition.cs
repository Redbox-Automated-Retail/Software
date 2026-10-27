using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "keyboard_email_change_view")]
public class KeyboardEmailChangedDefinition : BaseViewDefinition, IViewDefinition
{
	public KeyboardEmailChangedDefinition()
	{
		base.ViewName = "keyboard_email_change_view";
		base.ActiveFlag = true;
		base.ControlName = "KeyboardEmailChangeView";
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
