using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "vend_view")]
public class VendViewDefinition : BaseViewDefinition, IViewDefinition
{
	public VendViewDefinition()
	{
		base.ViewName = "vend_view";
		base.ActiveFlag = true;
		base.ControlName = "VendView";
		base.OnEnter = delegate
		{
			VendViewController.ConfigureVendView();
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			VendViewController.LeaveVendView(viewFrameInstance);
		};
	}
}
