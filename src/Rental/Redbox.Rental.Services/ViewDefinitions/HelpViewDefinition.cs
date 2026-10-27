using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "help_document_view")]
public class HelpViewDefinition : BaseViewDefinition, IViewDefinition
{
	public HelpViewDefinition()
	{
		base.ViewName = "help_document_view";
		base.ActiveFlag = true;
		base.ControlName = "HelpViewUserControl";
		base.Clear = true;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			HelpViewController.ConfigureHelpView(viewFrameInstance?.Parameter as HelpViewControllerParameters);
		};
		base.OnLeave = delegate
		{
			HelpViewController.LeaveHelpView();
		};
	}
}
