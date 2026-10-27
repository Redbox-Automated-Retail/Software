using System;

namespace Redbox.Rental.Services.KioskClientService.Ads;

public class ProofOfPlayRequest
{
	public DateTime DisplayedTime { get; set; }

	public int DisplayedMilliseconds { get; set; }
}
