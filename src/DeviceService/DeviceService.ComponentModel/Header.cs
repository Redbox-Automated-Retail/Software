namespace DeviceService.ComponentModel;

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
