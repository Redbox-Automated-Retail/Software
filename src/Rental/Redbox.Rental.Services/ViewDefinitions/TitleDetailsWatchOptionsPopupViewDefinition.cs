using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "title_details_watch_options_popup_view")]
public class TitleDetailsWatchOptionsPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public TitleDetailsWatchOptionsPopupViewDefinition()
	{
		base.ViewName = "title_details_watch_options_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "TitleDetailsWatchOptionsPopupView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			TitleDetailsWatchOptionsPopupViewController.ConfigureTitleDetailsWatchOptionsPopupView(viewFrameInstance?.Parameter as TitleDetailsWatchOptionsPopupViewParameter);
		};
	}
}
