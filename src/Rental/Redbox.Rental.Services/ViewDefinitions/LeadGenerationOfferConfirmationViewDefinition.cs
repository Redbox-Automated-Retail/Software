using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "lead_generation_offer_confirmation_view")]
public class LeadGenerationOfferConfirmationViewDefinition : BaseViewDefinition, IViewDefinition
{
	public LeadGenerationOfferConfirmationViewDefinition()
	{
		base.ViewName = "lead_generation_offer_confirmation_view";
		base.ActiveFlag = true;
		base.ControlName = "LeadGenerationOfferConfirmationView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			LeadGenerationOfferConfirmationViewController.ConfigureLeadGenerationOfferConfirmationView(viewFrameInstance?.Parameter as LeadGenerationOfferConfirmationViewParameters);
		};
		base.OnLeave = delegate
		{
			LeadGenerationOfferConfirmationViewController.LeaveLeadGenerationOfferConfirmationView();
		};
	}
}
