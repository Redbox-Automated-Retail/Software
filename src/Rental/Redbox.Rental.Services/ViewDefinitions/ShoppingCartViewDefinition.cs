using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.Environment;
using Redbox.Rental.Services.Controllers;

namespace Redbox.Rental.Services.ViewDefinitions;

[ViewDefinition(ViewName = "shopping_cart_view")]
public class ShoppingCartViewDefinition : BaseViewDefinition, IViewDefinition
{
	public ShoppingCartViewDefinition()
	{
		base.ViewName = "shopping_cart_view";
		base.ActiveFlag = true;
		base.ControlName = "ShoppingCartView";
		base.OnEnter = delegate
		{
			ShoppingCartViewController.ConfigureShoppingCartView();
		};
	}
}
