using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "application_load_view")]
public class ApplicationLoadViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ApplicationLoadViewDefinition()
	{
		base.ViewName = "application_load_view";
		base.ActiveFlag = true;
		base.ControlName = "ApplicationLoadView";
	}
}
