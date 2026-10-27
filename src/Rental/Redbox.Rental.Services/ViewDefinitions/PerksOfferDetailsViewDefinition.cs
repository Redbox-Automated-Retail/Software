using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "perks_offer_details_view")]
public class PerksOfferDetailsViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PerksOfferDetailsViewDefinition()
	{
		base.ViewName = "perks_offer_details_view";
		base.ActiveFlag = true;
		base.ControlName = "PerksOfferDetailsView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PerksOfferDetailsViewController.ConfigurePerksOfferDetailsView(viewFrameInstance?.Parameter as PerksOfferListItemModel);
		};
		base.OnLeave = delegate
		{
			PerksOfferDetailsViewController.LeavePerksOfferDetailsView();
		};
	}
}
