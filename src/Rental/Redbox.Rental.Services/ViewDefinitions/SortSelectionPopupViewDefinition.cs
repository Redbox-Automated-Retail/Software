using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "sort_selection_popup_view")]
public class SortSelectionPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public SortSelectionPopupViewDefinition()
	{
		base.ViewName = "sort_selection_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "SortSelectionPopupView";
		base.Clear = false;
		base.OnEnter = delegate
		{
			SortSelectionPopupViewController.ConfigureSortSelectionPopupView();
		};
		base.OnLeave = delegate
		{
			SortSelectionPopupViewController.LeaveSortSelectionPopupView();
		};
	}
}
