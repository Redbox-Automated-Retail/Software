using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "member_perks_view")]
public class MemberPerksDefinition : BaseViewDefinition, IViewDefinition
{
	public MemberPerksDefinition()
	{
		base.ViewName = "member_perks_view";
		base.ActiveFlag = true;
		base.ControlName = "MemberPerksView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			MemberPerksViewController.ConfigureView(viewFrameInstance?.Parameter as PerksMemberInfo);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			MemberPerksViewController.ClosingView(viewFrameInstance?.Parameter as PerksMemberInfo);
		};
	}
}
