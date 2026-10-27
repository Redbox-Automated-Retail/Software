using System;
using Redbox.Services.KioskBrokerServices.KioskShared.Enums;

namespace Redbox.Services.KioskBrokerServices.KioskShared.DomainObjects;

[Serializable]
public class DiscStatus
{
	public string Barcode { get; set; }

	public int DiscTypeID { get; set; }

	public DiscType DiscType
	{
		get
		{
			if (Enum.IsDefined(typeof(DiscType), DiscTypeID))
			{
				return (DiscType)Enum.ToObject(typeof(DiscType), DiscTypeID);
			}
			return DiscType._Unspecified;
		}
	}
}
