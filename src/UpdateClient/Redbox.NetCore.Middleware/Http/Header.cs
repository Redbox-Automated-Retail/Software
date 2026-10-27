namespace Redbox.NetCore.Middleware.Http;

public class Header
{
	public string Key { get; set; }

	public string Value { get; set; }

	public Header(string key, string value)
	{
		Key = key;
		Value = value;
	}
}
