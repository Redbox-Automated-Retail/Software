using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "title_details_full_details_popup_view")]
public class TitleDetailsFullDetailsPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public TitleDetailsFullDetailsPopupViewDefinition()
	{
		base.ViewName = "title_details_full_details_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "TitleDetailsFullDetailsPopupView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			TitleDetailsFullDetailsPopupViewController.ConfigureTitleDetailsFullDetailsPopupView(viewFrameInstance?.Parameter as TitleDetailsFullDetailsPopupViewInfo);
		};
	}
}
