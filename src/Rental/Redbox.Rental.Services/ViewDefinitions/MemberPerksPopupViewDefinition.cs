using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "member_perks_popup_view")]
public class MemberPerksPopupViewDefinition : BaseViewDefinition, IViewDefinition
{
	public MemberPerksPopupViewDefinition()
	{
		base.ViewName = "member_perks_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "MemberPerksPopupView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			MemberPerksPopupViewController.ConfigureView(viewFrameInstance?.Parameter as PerksPopupInfo);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			MemberPerksPopupViewController.ClosingView(viewFrameInstance?.Parameter as PerksPopupInfo);
		};
	}
}
