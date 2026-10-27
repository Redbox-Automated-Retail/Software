using System;
using System.Collections.Generic;
using Redbox.Rental.Model.KioskClientService.Inventory;
using Redbox.Rental.Services.App;

namespace Redbox.Rental.Services.KioskClientService.Inventory;

public class KioskTitlesCountResponse : ApiBaseResponse, IKioskTitlesCountResponse
{
	public Guid MessageId { get; set; }

	public List<_KioskTitlesCount> KioskTitlesCount { get; set; } = new List<_KioskTitlesCount>();
}
