using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "zip_code_view")]
public class ZipCodeViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ZipCodeViewDefinition()
	{
		base.ViewName = "zip_code_view";
		base.ActiveFlag = true;
		base.ControlName = "ZipCodeView";
		base.Clear = false;
		base.OnEnter = delegate(IViewFrameInstance viewFrameInstance)
		{
			ZipCodeViewController.ConfigureZipCodeView((viewFrameInstance.Parameter as ZipCodeViewParameter)?.ContinueAction);
		};
		base.OnLeave = delegate(IViewFrameInstance viewFrameInstance)
		{
			ZipCodeViewController.OnLeave(viewFrameInstance);
		};
	}
}
