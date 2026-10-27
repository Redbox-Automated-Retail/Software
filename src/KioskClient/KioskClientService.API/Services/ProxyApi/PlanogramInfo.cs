using System;

namespace KioskClientService.API.Services.ProxyApi;

public class PlanogramInfo
{
	public string Name { get; set; }

	public DateTime StartDate { get; set; }

	public string FileType { get; set; }

	public string Filename { get; set; }

	public DateTime LastUpdateUtc { get; set; }

	public string PresignedUrl { get; set; }
}
