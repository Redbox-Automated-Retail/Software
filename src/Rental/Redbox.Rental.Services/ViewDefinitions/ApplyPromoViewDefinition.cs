using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "apply_promo_view")]
public class ApplyPromoViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ApplyPromoViewDefinition()
	{
		base.ViewName = "apply_promo_view";
		base.ActiveFlag = true;
		base.ControlName = "ApplyPromoView";
		base.Clear = true;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			ApplyPromoViewController.ConfigureApplyPromoView(viewFrameInstance?.Parameter as ApplyPromoInfo);
		};
		base.OnLeave = delegate
		{
			ApplyPromoViewController.LeaveApplyPromoView();
		};
	}
}
