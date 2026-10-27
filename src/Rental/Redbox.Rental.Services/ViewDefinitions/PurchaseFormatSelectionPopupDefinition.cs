using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "format_filter_popup_view")]
public class PurchaseFormatSelectionPopupDefinition : BaseViewDefinition, IViewDefinition
{
	public PurchaseFormatSelectionPopupDefinition()
	{
		base.ViewName = "purchase_popup_view";
		base.ActiveFlag = true;
		base.ControlName = "FormatSelectionPopupView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			PurchaseFormatSelectionPopupViewController.ConfigureRentalFormatSelectionPopupView(viewFrameInstance?.Parameter as RentalFormatSelectionViewParameter);
		};
		base.OnLeave = delegate
		{
			PurchaseFormatSelectionPopupViewController.LeaveRentalFormatSelectionPopupView();
		};
	}
}
