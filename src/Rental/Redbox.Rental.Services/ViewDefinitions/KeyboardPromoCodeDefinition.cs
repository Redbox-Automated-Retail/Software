using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "keyboard_promo_view")]
public class KeyboardPromoCodeDefinition : BaseViewDefinition, IViewDefinition
{
	public KeyboardPromoCodeDefinition()
	{
		base.ViewName = "keyboard_promo_view";
		base.ActiveFlag = true;
		base.ControlName = "KeyboardPromoView";
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
