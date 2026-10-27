using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "option_selection_dialog_view")]
public class OptionSelectionDialogDefinition : BaseViewDefinition, IViewDefinition
{
	public OptionSelectionDialogDefinition()
	{
		base.ViewName = "option_selection_dialog_view";
		base.ActiveFlag = true;
		base.ControlName = "OptionSelectionDialog";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			OptionSelectionDialogController.ConfigureDialog((OptionSelectionDialogInfo)viewFrameInstance.Parameter);
		};
		base.OnLeave = delegate
		{
			OptionSelectionDialogController.LeaveDialog();
		};
	}
}
