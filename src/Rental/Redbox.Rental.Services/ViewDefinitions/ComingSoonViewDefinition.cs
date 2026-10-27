using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "coming_soon_view")]
public class ComingSoonViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ComingSoonViewDefinition()
	{
		base.ViewName = "coming_soon_view";
		base.ActiveFlag = true;
		base.ControlName = "ComingSoonView";
		base.OnEnter = delegate
		{
			ComingSoonViewController.ConfigureComingSoonView();
		};
	}
}
