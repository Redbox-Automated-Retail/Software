namespace DeviceService.ComponentModel;

public class ObfuscationHelper
{
	public static string ObfuscateValue(string value)
	{
		if (!string.IsNullOrEmpty(value))
		{
			return $"[Value not logged, length: {value?.Length ?? 0}]";
		}
		return null;
	}
}
