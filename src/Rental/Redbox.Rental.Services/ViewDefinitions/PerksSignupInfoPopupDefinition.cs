using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "perks_signup_info_popup_view")]
public class PerksSignupInfoPopupDefinition : BaseViewDefinition, IViewDefinition
{
	public PerksSignupInfoPopupDefinition()
	{
		base.ViewName = "perks_signup_info_popup_view";
		base.ActiveFlag = true;
		base.Clear = false;
		base.ControlName = "PerksSignUpInfoPopupView";
		base.OnEnter = delegate
		{
			PerksSignUpInfoPopupViewController.ConfigureView();
		};
	}
}
