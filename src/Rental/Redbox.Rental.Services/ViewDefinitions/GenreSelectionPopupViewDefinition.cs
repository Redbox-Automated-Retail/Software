using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "genre_selection_popup_view")]
public class GenreSelectionPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public GenreSelectionPopupViewDefinition()
	{
		base.ViewName = "genre_selection_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "GenreSelectionPopupView";
		base.Clear = false;
		base.OnEnter = delegate
		{
			GenreSelectionPopupViewController.ConfigureGenreSelectionPopupView();
		};
		base.OnLeave = delegate
		{
			GenreSelectionPopupViewController.LeaveGenreSelectionPopupView();
		};
	}
}
