using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "remove_card_view")]
public class RemoveCardViewDefinition : BaseViewDefinition, IViewDefinition
{
	public RemoveCardViewDefinition()
	{
		base.ViewName = "remove_card_view";
		base.ActiveFlag = true;
		base.ControlName = "RemoveCardView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			RemoveCardViewController.ConfigureView(viewFrameInstance.Parameter as RemoveCardViewParameters);
		};
		base.OnLeave = delegate
		{
			RemoveCardViewController.LeaveView();
		};
	}
}
