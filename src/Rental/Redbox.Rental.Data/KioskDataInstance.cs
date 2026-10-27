using System;
using Redbox.Rental.Model.KioskData;

namespace Redbox.Rental.Data;

public class KioskDataInstance : IKioskDataInstance
{
	public DateTime VersionTimeStamp { get; set; }

	public KioskDataInstance()
	{
		VersionTimeStamp = new DateTime(2000, 1, 1);
	}
}
