using System;

namespace Redbox.Rental.Model.KioskData;

public interface IKioskDataInstance
{
	DateTime VersionTimeStamp { get; set; }
}
