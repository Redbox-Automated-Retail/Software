using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model.KioskProduct;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "redbox_plus_offer_detail_view")]
public class RedboxPlusOfferDetailViewDefinition : BaseViewDefinition, IViewDefinition
{
	public RedboxPlusOfferDetailViewDefinition()
	{
		base.ViewName = "redbox_plus_offer_detail_view";
		base.ActiveFlag = true;
		base.ControlName = "RedboxPlusOfferDetailView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			RedboxPlusOfferDetailViewController.ConfigureRedboxPlusOfferDetailView(viewFrameInstance.Parameter as IRedboxPlusSubscriptionProduct);
		};
	}
}
