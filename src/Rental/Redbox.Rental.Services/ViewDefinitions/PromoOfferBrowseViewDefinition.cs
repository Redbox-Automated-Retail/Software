using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "promo_offer_browse_view")]
public class PromoOfferBrowseViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PromoOfferBrowseViewDefinition()
	{
		base.ViewName = "promo_offer_browse_view";
		base.ActiveFlag = true;
		base.ControlName = "PromoOfferBrowseView";
		base.Clear = true;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PromoOfferBrowseViewController.ConfigureView(viewFrameInstance?.Parameter as PromoOfferBrowseInfo);
		};
		base.OnLeave = delegate
		{
			PromoOfferBrowseViewController.LeaveView();
		};
	}
}
