using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "perks_info_popup_view")]
public class PerksInfoPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public PerksInfoPopupViewDefinition()
	{
		base.ViewName = "perks_info_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "BulletListPopupView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			BulletListPopupViewController.ConfigurePerksInfoView(viewFrameInstance?.Parameter as BulletListPopupParameters);
		};
		base.OnLeave = delegate
		{
			BulletListPopupViewController.LeavePerksInfoView();
		};
	}
}
