using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "animated_disc_return")]
public class AnimatedDiscReturnDefinition : BaseViewDefinition, IViewDefinition
{
	public AnimatedDiscReturnDefinition()
	{
		base.ViewName = "animated_disc_return";
		base.ActiveFlag = true;
		base.ControlName = "AnimatedDiscReturnViewUserControl";
		base.OnEnter = delegate
		{
			AnimatedDiscReturnViewController.ConfigureAnimatedDiscReturnView();
		};
	}
}
