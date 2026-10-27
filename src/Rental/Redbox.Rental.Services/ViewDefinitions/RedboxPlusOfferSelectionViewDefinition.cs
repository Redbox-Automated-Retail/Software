using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "redbox_plus_offer_selection_view")]
public class RedboxPlusOfferSelectionViewDefinition : BaseViewDefinition, IViewDefinition
{
	public RedboxPlusOfferSelectionViewDefinition()
	{
		base.ViewName = "redbox_plus_offer_selection_view";
		base.ActiveFlag = true;
		base.ControlName = "RedboxPlusOfferSelectionView";
		base.OnEnter = delegate
		{
			RedboxPlusOfferSelectionViewController.ConfigureView();
		};
	}
}
