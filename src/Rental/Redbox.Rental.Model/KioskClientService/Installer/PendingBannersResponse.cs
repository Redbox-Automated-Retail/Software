using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskClientService.Installer;

public class PendingBannersResponse
{
	public bool Success { get; set; }

	public int StatusCode { get; set; }

	public List<Error> Errors { get; set; } = new List<Error>();

	public List<Banner> Banners { get; set; } = new List<Banner>();
}
