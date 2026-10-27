namespace KioskClientService.API.App;

public static class HttpConstants
{
	public static class Headers
	{
		public static class ContentEncodingValues
		{
			public const string GZip = "gzip";

			public const string Deflate = "deflate";

			public static bool Contains(string value)
			{
				if (!(value == "gzip"))
				{
					return value == "deflate";
				}
				return true;
			}
		}

		public const string ContentEncoding = "Content-Encoding";
	}
}
