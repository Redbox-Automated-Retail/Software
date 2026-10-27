using System.Collections.Generic;

namespace KioskClientService.API.Vistar;

public class VistarDisplayArea
{
	public string id { get; set; }

	public List<string> supported_media { get; set; }

	public bool allow_audio { get; set; }

	public int width { get; set; }

	public int height { get; set; }

	public int? min_duration { get; set; }

	public int? max_duration { get; set; }

	public string order_id { get; set; }

	public int? max_file_size_bytes { get; set; }

	public int? static_duration { get; set; }
}
