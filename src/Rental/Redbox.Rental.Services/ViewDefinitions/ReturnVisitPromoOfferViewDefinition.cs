using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "return_visit_promo_offer_view")]
public class ReturnVisitPromoOfferViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ReturnVisitPromoOfferViewDefinition()
	{
		base.ViewName = "return_visit_promo_offer_view";
		base.ActiveFlag = true;
		base.ControlName = "ReturnVisitPromoOfferView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			ReturnVisitPromoOfferViewController.ConfigureReturnVisitPromoOfferView(viewFrameInstance?.Parameter as ReturnVisitPromoOfferViewParameters);
		};
		base.OnLeave = delegate
		{
			ReturnVisitPromoOfferViewController.LeaveReturnVisitPromoOfferView();
		};
	}
}
