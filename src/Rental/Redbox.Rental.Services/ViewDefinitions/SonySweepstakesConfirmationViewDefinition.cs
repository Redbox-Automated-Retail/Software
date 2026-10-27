using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "sony_sweepstakes_confirmation_view")]
public class SonySweepstakesConfirmationViewDefinition : BaseViewDefinition, IViewDefinition
{
	public SonySweepstakesConfirmationViewDefinition()
	{
		base.ViewName = "sony_sweepstakes_confirmation_view";
		base.ActiveFlag = true;
		base.ControlName = "SonySweepstakesConfirmationView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.ConfigureSonySweepstakesConfirmationView(viewFrameInstance?.Parameter as SonySweepstakesConfirmationViewInfo);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.OnLeaveSonySweepstakesConfirmationView(viewFrameInstance);
		};
	}
}
