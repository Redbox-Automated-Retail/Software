using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "upsell_view")]
public class UpsellViewDefinition : BaseViewDefinition, IViewDefinition
{
	public UpsellViewDefinition()
	{
		base.ViewName = "upsell_view";
		base.ActiveFlag = true;
		base.Clear = false;
		base.ControlName = "UpsellView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			if (viewFrameInstance.Parameter is ShowUpsellViewParameter showUpsellViewParameter)
			{
				UpsellViewController.ConfigureUpsellView(showUpsellViewParameter.TitleType);
			}
		};
		base.OnLeave = delegate
		{
			UpsellViewController.CleanupUpsellView();
		};
	}
}
