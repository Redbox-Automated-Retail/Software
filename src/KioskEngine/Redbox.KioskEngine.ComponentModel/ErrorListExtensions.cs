namespace Redbox.KioskEngine.ComponentModel;

public static class ErrorListExtensions
{
	public static bool IsNotNullOrEmpty(this ErrorList list)
	{
		if (list != null)
		{
			return list.Count > 0;
		}
		return false;
	}

	public static bool IsNullOrEmpty(this ErrorList list)
	{
		if (list != null)
		{
			return list.Count == 0;
		}
		return true;
	}
}
