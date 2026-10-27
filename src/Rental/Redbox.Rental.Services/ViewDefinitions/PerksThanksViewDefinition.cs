using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "perks_thanks_view")]
public class PerksThanksViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PerksThanksViewDefinition()
	{
		base.ViewName = "perks_thanks_view";
		base.ActiveFlag = true;
		base.ControlName = "PerksThanksView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PerksThanksViewController.ConfigureView(viewFrameInstance?.Parameter as PerksThanksInfo);
		};
	}
}
