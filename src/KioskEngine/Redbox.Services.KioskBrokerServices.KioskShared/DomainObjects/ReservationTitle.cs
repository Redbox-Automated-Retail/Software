using System;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class ReservationTitle
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
}
