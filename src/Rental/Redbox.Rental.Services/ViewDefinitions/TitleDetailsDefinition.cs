using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "product_detail_view")]
public class TitleDetailsDefinition : BaseViewDefinition, IViewDefinition
{
	public TitleDetailsDefinition()
	{
		base.ViewName = "product_detail_view";
		base.ActiveFlag = true;
		base.ControlName = "TitleDetailsView";
		base.Clear = false;
		base.OnEnter = delegate
		{
			TitleDetailsController.ConfigureView();
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			TitleDetailsController.OnLeave(viewFrameInstance);
		};
	}
}
