using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "avod_lead_generation_offer_view")]
public class AvodLeadGenerationOfferViewDefinition : BaseViewDefinition, IViewDefinition
{
	public AvodLeadGenerationOfferViewDefinition()
	{
		base.ViewName = "avod_lead_generation_offer_view";
		base.ActiveFlag = true;
		base.ControlName = "AvodLeadGenerationOfferView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			AvodLeadGenerationOfferViewController.ConfigureAvodLeadGenerationOfferView(viewFrameInstance?.Parameter as AvodLeadGenerationOfferViewParameters);
		};
		base.OnLeave = delegate
		{
			AvodLeadGenerationOfferViewController.LeaveAvodLeadGenerationOfferView();
		};
	}
}
