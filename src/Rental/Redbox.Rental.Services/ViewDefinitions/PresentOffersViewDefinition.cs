using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "PresentOffersView")]
public class PresentOffersViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PresentOffersViewDefinition()
	{
		base.ViewName = "PresentOffersView";
		base.ActiveFlag = true;
		base.ControlName = "PresentOffersView";
		base.Clear = true;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PresentOffersViewController.ConfigureView(viewFrameInstance?.Parameter as PresentOffersViewInfo);
		};
		base.OnLeave = delegate
		{
			PresentOffersViewController.LeaveView();
		};
	}
}
