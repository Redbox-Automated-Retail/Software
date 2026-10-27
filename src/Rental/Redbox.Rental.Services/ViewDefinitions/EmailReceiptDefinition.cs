using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "email_receipt_view")]
public class EmailReceiptDefinition : BaseViewDefinition, IViewDefinition
{
	public EmailReceiptDefinition()
	{
		base.ViewName = "email_receipt_view";
		base.ActiveFlag = true;
		base.ControlName = "EmailReceiptView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.ConfigureEmailReceiptView(viewFrameInstance?.Parameter as EmailReceiptInfo);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			SimpleViewController.OnLeaveEmailReceiptView(viewFrameInstance);
		};
	}
}
