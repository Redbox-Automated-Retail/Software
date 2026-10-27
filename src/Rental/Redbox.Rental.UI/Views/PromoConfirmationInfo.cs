using System;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class PromoConfirmationInfo
{
	public StoredPromoCodeModel promoCodeModel { get; set; }

	public Action<string> ApplyPromoCodeAction { get; set; }

	public Action CancelAction { get; set; }
}
