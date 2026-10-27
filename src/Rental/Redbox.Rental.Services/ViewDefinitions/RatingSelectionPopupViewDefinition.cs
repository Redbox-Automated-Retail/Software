using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "rating_selection_popup_view")]
public class RatingSelectionPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public RatingSelectionPopupViewDefinition()
	{
		base.ViewName = "rating_selection_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "RatingSelectionPopupView";
		base.Clear = false;
		base.OnEnter = delegate
		{
			RatingSelectionPopupViewController.ConfigureRatingSelectionPopupView();
		};
		base.OnLeave = delegate
		{
			RatingSelectionPopupViewController.LeaveRatingSelectionPopupView();
		};
	}
}
