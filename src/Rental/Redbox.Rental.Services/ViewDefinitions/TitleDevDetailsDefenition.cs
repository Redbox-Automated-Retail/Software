using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "title_details_dev_view")]
internal class TitleDevDetailsDefenition : BaseViewDefinition, IViewDefinition
{
	public TitleDevDetailsDefenition()
	{
		base.ViewName = "title_details_dev_view";
		base.ActiveFlag = true;
		base.ControlName = "TitleDetailsDevView";
		base.Clear = false;
		base.OnEnter = delegate
		{
			TitleDevDetailsController.ConfigureView();
		};
	}
}
