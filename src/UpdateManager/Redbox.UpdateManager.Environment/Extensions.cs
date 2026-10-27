using System;
using System.Text;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Environment;

public static class Extensions
{
	public static void ToLogHelper(this ErrorList errors)
	{
		if (errors.Count > 0)
		{
			LogHelper.Instance.Log(errors.ToSingleString());
		}
	}

	public static string ToSingleString(this ErrorList errors)
	{
		StringBuilder stringBuilder = new StringBuilder();
		foreach (Redbox.UpdateManager.ComponentModel.Error error in errors)
		{
			stringBuilder.AppendLine(error.ToString());
			stringBuilder.AppendLine(error.Details);
		}
		return stringBuilder.ToString();
	}

	public static bool TryParse<T>(string value, out T result, bool ignoreCase) where T : struct
	{
		result = default(T);
		try
		{
			result = (T)Enum.Parse(typeof(T), value, ignoreCase);
			return true;
		}
		catch
		{
		}
		return false;
	}
}
