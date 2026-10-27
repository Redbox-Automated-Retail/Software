namespace KioskClientService.API.Ads;

public static class MediaTypeNames
{
	public const string Jpeg = "image/jpeg";

	public const string Png = "image/png";

	public static string GetExt(string mt)
	{
		if (mt == "image/jpeg")
		{
			return "jpg";
		}
		if (mt == "image/png")
		{
			return "png";
		}
		return "jpg";
	}
}
