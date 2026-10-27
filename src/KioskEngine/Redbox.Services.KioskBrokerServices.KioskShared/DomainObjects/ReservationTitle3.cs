using System;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class ReservationTitle3
{
	public int ItemID { get; set; }

	public int TitleID { get; set; }

	public ReservationType ReservationType { get; set; }

	public decimal Price { get; set; }

	public decimal Discount { get; set; }

	public DiscountType DiscountType { get; set; }

	public decimal DiscountedPrice { get; set; }

	public int NumberOfCredits { get; set; }

	public decimal LoyaltyPoints { get; set; }
}
