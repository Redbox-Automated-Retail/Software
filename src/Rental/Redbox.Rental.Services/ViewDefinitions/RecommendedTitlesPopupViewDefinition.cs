using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "recommended_titles_popup_view")]
public class RecommendedTitlesPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public RecommendedTitlesPopupViewDefinition()
	{
		base.ViewName = "recommended_titles_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "RecommendedTitlesPopupView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			RecommendedTitlesPopupViewController.ConfigureRecommendedTitlesPopupView(viewFrameInstance?.Parameter as RecommendedTitlesPopupInfo);
		};
		base.OnLeave = delegate
		{
			RecommendedTitlesPopupViewController.LeaveRecommendedTitlesPopupView();
		};
	}
}
