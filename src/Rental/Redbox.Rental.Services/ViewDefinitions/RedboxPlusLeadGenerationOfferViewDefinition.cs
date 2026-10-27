using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "redbox_plus_lead_generation_offer_view")]
public class RedboxPlusLeadGenerationOfferViewDefinition : BaseViewDefinition, IViewDefinition
{
	public RedboxPlusLeadGenerationOfferViewDefinition()
	{
		base.ViewName = "redbox_plus_lead_generation_offer_view";
		base.ActiveFlag = true;
		base.ControlName = "RedboxPlusLeadGenerationOfferView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			RedboxPlusLeadGenerationOfferViewController.ConfigureRedboxPlusLeadGenerationOfferView(viewFrameInstance?.Parameter as RedboxPlusLeadGenerationOfferViewParameters);
		};
		base.OnLeave = delegate
		{
			RedboxPlusLeadGenerationOfferViewController.LeaveRedboxPlusLeadGenerationOfferView();
		};
	}
}
