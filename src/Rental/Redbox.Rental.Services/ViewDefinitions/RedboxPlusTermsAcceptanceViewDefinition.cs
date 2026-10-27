using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "redbox_plus_terms_acceptance_view")]
public class RedboxPlusTermsAcceptanceViewDefinition : BaseViewDefinition, IViewDefinition
{
	public RedboxPlusTermsAcceptanceViewDefinition()
	{
		base.ViewName = "redbox_plus_terms_acceptance_view";
		base.ActiveFlag = true;
		base.ControlName = "RedboxPlusTermsAcceptanceView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			RedboxPlusTermsAcceptanceViewController.ConfigureRedboxPlusTermsAcceptanceView(viewFrameInstance.Parameter as RedboxPlusTermsAcceptanceViewParameters);
		};
	}
}
