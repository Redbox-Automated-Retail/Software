using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "reserve_online_popup_view")]
public class ReserveOnlinePopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ReserveOnlinePopupViewDefinition()
	{
		base.ViewName = "reserve_online_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "BulletListPopupView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			BulletListPopupViewController.ConfigureRedBoxAppInfoView(viewFrameInstance?.Parameter as BulletListPopupParameters);
		};
		base.OnLeave = delegate
		{
			BulletListPopupViewController.LeaveRedboxAppInfoView();
		};
	}
}
