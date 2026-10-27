using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "OfferAbandonConfirmView")]
public class OfferAbandonConfirmViewDefinition : BaseViewDefinition, IViewDefinition
{
	public OfferAbandonConfirmViewDefinition()
	{
		base.ViewName = "OfferAbandonConfirmView";
		base.ActiveFlag = true;
		base.ControlName = "OfferAbandonConfirmView";
		base.Clear = true;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			OfferAbandonConfirmViewController.ConfigureView(viewFrameInstance?.Parameter as OfferAbandonConfirmInfo);
		};
		base.OnLeave = delegate
		{
			OfferAbandonConfirmViewController.LeaveView();
		};
	}
}
