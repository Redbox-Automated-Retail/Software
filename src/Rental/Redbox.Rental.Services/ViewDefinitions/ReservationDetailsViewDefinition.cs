using System;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "reservation_details_view")]
public class ReservationDetailsViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ReservationDetailsViewDefinition()
	{
		base.ViewName = "reservation_details_view";
		base.ActiveFlag = true;
		base.ControlName = "ReservationDetailsView";
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			ReservationDetailsViewController.ConfigureView(viewFrameInstance.Parameter as Action);
		};
		base.OnLeave = delegate
		{
			ReservationDetailsViewController.LeaveView();
		};
	}
}
