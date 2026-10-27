using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Model.Promotion;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "new_cart_confirm_ada_action_update_view")]
public class NewCartConfirmADAActionUpdateViewDefinition : BaseViewDefinition, IViewDefinition
{
	public NewCartConfirmADAActionUpdateViewDefinition()
	{
		base.ViewName = "new_cart_confirm_ada_action_update_view";
		base.ActiveFlag = true;
		base.ControlName = "NewCartConfirmADAActionUpdateView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			NewCartConfirmADAActionUpdateViewController.ConfigureView((CustomerOffer)viewFrameInstance.Parameter);
		};
		base.OnLeave = delegate
		{
			NewCartConfirmADAActionUpdateViewController.LeaveView();
		};
	}
}
