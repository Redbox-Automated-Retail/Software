using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Installer;

public class PendingKiosksResponse
{
	public bool Success { get; set; }

	public int StatusCode { get; set; }

	public List<Error> Errors { get; set; } = new List<Error>();

	public List<PendingKiosk> PendingKiosks { get; set; } = new List<PendingKiosk>();
}
