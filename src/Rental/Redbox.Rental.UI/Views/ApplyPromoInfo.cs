using System;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class ApplyPromoInfo
{
	public Action<string> ApplyPromoCodeAction { get; set; }

	private ApplyPromoViewModel ViewModel { get; set; }
}
