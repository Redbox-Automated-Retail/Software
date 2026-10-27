using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "please_wait_message_view")]
public class PleaseWaitMessageViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PleaseWaitMessageViewDefinition()
	{
		base.ViewName = "please_wait_message_view";
		base.ControlName = "PleaseWaitMessageView";
		base.OnEnter = delegate
		{
			PleaseWaitMessageViewController.ConfigureView();
		};
	}
}
