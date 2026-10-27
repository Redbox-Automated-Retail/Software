using System;
using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Kiosk;

public interface IKioskClientServiceNearbyKiosksResponse
{
	Guid MessageId { get; }

	List<NearbyKiosk> Kiosks { get; }
}
