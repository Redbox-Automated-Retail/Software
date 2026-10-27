using System;

namespace UpdateClientService.API.App;

public static class ExceptionExtensions
{
	public static string GetFullMessage(this Exception e)
	{
		if (e != null)
		{
			if (e.InnerException != null)
			{
				return e.Message.Replace("\n", "").Replace("\r", "") + " -> " + e.InnerException.GetFullMessage();
			}
			return e.Message;
		}
		return "";
	}
}
