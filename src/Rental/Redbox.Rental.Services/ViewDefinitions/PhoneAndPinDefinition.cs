using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "phone_and_pin_view")]
public class PhoneAndPinDefinition : BaseViewDefinition, IViewDefinition
{
	public PhoneAndPinDefinition()
	{
		base.ViewName = "phone_and_pin_view";
		base.ActiveFlag = true;
		base.ControlName = "PhoneAndPinView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PhoneAndPinViewController.ConfigurePhoneAndPinView(viewFrameInstance?.Parameter as PhoneAndPinInfo);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			PhoneAndPinViewController.LeavePhoneAndPinView(viewFrameInstance?.Parameter as PhoneAndPinInfo);
		};
	}
}
