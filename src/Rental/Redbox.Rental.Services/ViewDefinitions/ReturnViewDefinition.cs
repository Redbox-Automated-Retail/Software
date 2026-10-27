using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "return_view")]
public class ReturnViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ReturnViewDefinition()
	{
		base.ViewName = "return_view";
		base.ActiveFlag = true;
		base.ControlName = "ReturnView";
		base.OnEnter = delegate
		{
			ReturnViewController.ConfigureReturnView();
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			ReturnViewController.OnLeave(viewFrameInstance);
		};
	}
}
