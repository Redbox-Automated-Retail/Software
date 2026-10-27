using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "multi_disc_vend_view")]
public class MultiDiscVendViewDefinition : BaseViewDefinition, IViewDefinition
{
	public MultiDiscVendViewDefinition()
	{
		base.ViewName = "multi_disc_vend_view";
		base.ActiveFlag = true;
		base.ControlName = "MultiDiscVendView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			MultiDiscVendViewController.ConfigureView(viewFrameInstance.Parameter as Action);
		};
		base.OnLeave = delegate
		{
			MultiDiscVendViewController.LeaveView();
		};
	}
}
