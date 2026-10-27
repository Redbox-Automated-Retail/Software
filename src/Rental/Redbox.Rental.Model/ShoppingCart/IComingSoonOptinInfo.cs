using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.Rental.Model.ShoppingCart;

public interface IComingSoonOptinInfo
{
	string EmailAddress { get; set; }

	List<IOptInItem> OptInItems { get; }

	void AddProduct(long productId);

	void RemoveProduct(long productId);
}
