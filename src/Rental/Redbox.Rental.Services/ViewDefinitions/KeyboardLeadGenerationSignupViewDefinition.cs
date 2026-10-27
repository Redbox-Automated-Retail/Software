using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "keyboard_lead_generation_signup_view")]
public class KeyboardLeadGenerationSignupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public KeyboardLeadGenerationSignupViewDefinition()
	{
		base.ViewName = "keyboard_lead_generation_signup_view";
		base.ActiveFlag = true;
		base.ControlName = "KeyboardLeadGenerationSignupView";
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
