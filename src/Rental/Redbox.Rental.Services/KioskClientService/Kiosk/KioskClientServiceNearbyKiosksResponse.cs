using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskClientService.Kiosk;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Kiosk;

public class KioskClientServiceNearbyKiosksResponse : ApiBaseResponse, IKioskClientServiceNearbyKiosksResponse
{
	public Guid MessageId { get; set; }

	public List<NearbyKiosk> Kiosks { get; set; } = new List<NearbyKiosk>();
}
