using Redbox.FMA.Controllers;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;

namespace Redbox.FMA.ViewDefinitions;

[ViewDefinition(ViewName = "select_planogram_view")]
public class SelectPlanogramViewDefinition : BaseViewDefinition, IViewDefinition
{
	public SelectPlanogramViewDefinition()
	{
		base.ViewName = "select_planogram_view";
		base.ActiveFlag = true;
		base.ControlName = "SelectPlanogramView";
		base.Clear = true;
		base.OnEnter = delegate
		{
			SelectPlanogramViewController.ConfigureView();
		};
	}
}
