using System;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class ReservationTitle2
{
	public int ItemID { get; set; }

	public int TitleID { get; set; }

	public int ReservationTypeID { get; set; }

	public ReservationType ReservationType
	{
		get
		{
			if (Enum.IsDefined(typeof(ReservationType), ReservationTypeID))
			{
				return (ReservationType)Enum.ToObject(typeof(ReservationType), ReservationTypeID);
			}
			return ReservationType._Unspecified;
		}
	}

	public int DiscFormatID { get; set; }

	public DiscFormat DiscFormat
	{
		get
		{
			if (Enum.IsDefined(typeof(DiscFormat), DiscFormatID))
			{
				return (DiscFormat)Enum.ToObject(typeof(DiscFormat), DiscFormatID);
			}
			return DiscFormat._Unspecified;
		}
	}

	public decimal Price { get; set; }

	public decimal Discount { get; set; }

	public decimal DiscountedPrice { get; set; }

	public int NumberOfCredits { get; set; }

	public long? CreditID { get; set; }
}
