using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "redbox_plus_info_popup_view")]
public class RedboxPlusInfoPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public RedboxPlusInfoPopupViewDefinition()
	{
		base.ViewName = "redbox_plus_info_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "RedboxPlusInfoPopupView";
		base.Clear = false;
		base.OnEnter = delegate
		{
			RedboxPlusInfoPopupViewController.ConfigureRedboxPlusInfoPopupView();
		};
	}
}
