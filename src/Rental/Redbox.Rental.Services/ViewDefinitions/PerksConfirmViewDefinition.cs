using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "perks_confirm_view")]
public class PerksConfirmViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PerksConfirmViewDefinition()
	{
		base.ViewName = "perks_confirm_view";
		base.ActiveFlag = true;
		base.ControlName = "PerksConfirmView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PerksConfirmViewController.ConfigureView(viewFrameInstance?.Parameter as PerksConfirmViewParameters);
		};
	}
}
