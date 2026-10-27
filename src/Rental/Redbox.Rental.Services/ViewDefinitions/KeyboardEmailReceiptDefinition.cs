using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "keyboard_email_receipt_view")]
public class KeyboardEmailReceiptDefinition : BaseViewDefinition, IViewDefinition
{
	public KeyboardEmailReceiptDefinition()
	{
		base.ViewName = "keyboard_email_receipt_view";
		base.ActiveFlag = true;
		base.ControlName = "KeyboardEmailReceiptView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			KeyboardViewController.ConfigureView(viewFrameInstance?.Parameter);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			KeyboardViewController.ClosingView(viewFrameInstance?.Parameter);
		};
	}
}
