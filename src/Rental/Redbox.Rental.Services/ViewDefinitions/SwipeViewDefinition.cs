using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "swipe_view")]
public class SwipeViewDefinition : BaseViewDefinition, IViewDefinition
{
	public SwipeViewDefinition()
	{
		base.ViewName = "swipe_view";
		base.ActiveFlag = true;
		base.ControlName = "SwipeView";
		base.OnEnter = delegate
		{
			SwipeViewController.ConfigureSwipeView();
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			SwipeViewController.LeaveSwipeView(viewFrameInstance);
		};
	}
}
