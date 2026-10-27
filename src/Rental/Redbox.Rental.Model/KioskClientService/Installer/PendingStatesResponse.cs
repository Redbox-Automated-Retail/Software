using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Installer;

public class PendingStatesResponse
{
	public bool Success { get; set; }

	public int StatusCode { get; set; }

	public List<Error> Errors { get; set; } = new List<Error>();

	public List<State> States { get; set; } = new List<State>();
}
