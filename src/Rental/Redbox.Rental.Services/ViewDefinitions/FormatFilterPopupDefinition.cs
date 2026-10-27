using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "format_filter_popup_view")]
public class FormatFilterPopupDefinition : BaseViewDefinition, IViewDefinition
{
	public FormatFilterPopupDefinition()
	{
		base.ViewName = "format_filter_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "FormatSelectionPopupView";
		base.Clear = false;
		base.OnEnter = delegate
		{
			FormatFilterPopupViewController.ConfigureFormatFilterPopupView();
		};
		base.OnLeave = delegate
		{
			FormatFilterPopupViewController.LeaveFormatFilterPopupView();
		};
	}
}
