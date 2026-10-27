using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "promo_confirmation_view")]
public class PromoConfirmationViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PromoConfirmationViewDefinition()
	{
		base.ViewName = "promo_confirmation_view";
		base.ActiveFlag = true;
		base.ControlName = "PromoConfirmationView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PromoConfirmationViewController.ConfigurePromoConfirmationView(viewFrameInstance?.Parameter as PromoConfirmationInfo);
		};
		base.OnLeave = delegate
		{
			PromoConfirmationViewController.LeavePromoConfirmationView();
		};
	}
}
