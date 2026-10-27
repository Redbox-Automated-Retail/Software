using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "keyboard_email_coming_soon_view")]
public class KeyboardComingSoonDefinition : BaseViewDefinition, IViewDefinition
{
	public KeyboardComingSoonDefinition()
	{
		base.ViewName = "keyboard_email_coming_soon_view";
		base.ActiveFlag = true;
		base.ControlName = "KeyboardEmailComingSoonView";
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
