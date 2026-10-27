namespace KioskClientService.API.Vistar;

public class VistarExpirationResponse
{
	public float impressions { get; set; }

	public int media_cost { get; set; }

	public int spots { get; set; }

	public int expires { get; set; }

	public int errors { get; set; }

	public string message { get; set; }
}
