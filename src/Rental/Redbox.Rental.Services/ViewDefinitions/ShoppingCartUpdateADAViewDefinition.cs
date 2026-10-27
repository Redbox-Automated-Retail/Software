using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "shopping_cart_update_ada_view")]
public class ShoppingCartUpdateADAViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ShoppingCartUpdateADAViewDefinition()
	{
		base.ViewName = "shopping_cart_update_ada_view";
		base.ActiveFlag = true;
		base.Clear = false;
		base.ControlName = "ShoppingCartUpdateADAView";
		base.OnEnter = delegate
		{
			ShoppingCartUpdateADAViewController.ConfigureView();
		};
		base.OnLeave = delegate
		{
			ShoppingCartUpdateADAViewController.LeaveView();
		};
	}
}
