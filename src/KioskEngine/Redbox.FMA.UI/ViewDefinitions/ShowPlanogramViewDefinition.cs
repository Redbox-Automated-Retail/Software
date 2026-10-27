using Redbox.FMA.Controllers;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;

namespace Redbox.FMA.ViewDefinitions;

[ViewDefinition(ViewName = "show_planogram_view")]
public class ShowPlanogramViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ShowPlanogramViewDefinition()
	{
		base.ViewName = "show_planogram_view";
		base.ActiveFlag = true;
		base.ControlName = "ShowPlanogramView";
		base.OnEnter = delegate
		{
			ShowPlanogramViewController.ConfigureView();
		};
	}
}
