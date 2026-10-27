using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "new_cart_confirm_view")]
public class NewCartConfirmViewDefinition : BaseViewDefinition, IViewDefinition
{
	public NewCartConfirmViewDefinition()
	{
		base.ViewName = "new_cart_confirm_view";
		base.ActiveFlag = true;
		base.ControlName = "NewCartConfirmView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			NewCartConfirmViewController.ConfigureView((NewCartConfirmviewParameters)viewFrameInstance.Parameter);
		};
		base.OnLeave = delegate
		{
			NewCartConfirmViewController.LeaveView();
		};
	}
}
