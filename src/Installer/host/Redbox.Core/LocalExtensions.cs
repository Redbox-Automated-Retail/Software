using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core;

internal static class LocalExtensions
{
	public static void AppendErrors(this StringBuilder sb, List<Error> errors)
	{
		if (errors.Count != 0)
		{
			sb.Append(", Errors:");
			sb.Append(EnumerableExtensions.Join(errors, "; "));
		}
	}

	public static Error MakeError(string code, string description, Exception e)
	{
		Error error = new Error();
		error.Code = code;
		error.Description = description;
		Error error2 = error;
		if (e != null)
		{
			error2.ExceptionType = e.GetType().FullName;
			error2.ExceptionMessage = e.Message;
			error2.ExceptionStackTrace = e.StackTrace;
		}
		return error2;
	}
}
