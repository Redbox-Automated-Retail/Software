using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "member_points_view")]
public class MemberPointsDefinition : BaseViewDefinition, IViewDefinition
{
	public MemberPointsDefinition()
	{
		base.ViewName = "member_points_view";
		base.ActiveFlag = true;
		base.ControlName = "MemberPointsView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			MemberPointsViewController.ConfigureView(viewFrameInstance?.Parameter as PointsMemberInfo);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			MemberPointsViewController.ClosingView(viewFrameInstance?.Parameter as PointsMemberInfo);
		};
	}
}
