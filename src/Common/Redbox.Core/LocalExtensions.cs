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
			sb.Append(errors.Join("; "));
		}
	}

	public static Error MakeError(string code, string description, Exception e)
	{
		Error error = new Error
		{
			Code = code,
			Description = description
		};
		if (e != null)
		{
			error.ExceptionType = e.GetType().FullName;
			error.ExceptionMessage = e.Message;
			error.ExceptionStackTrace = e.StackTrace;
		}
		return error;
	}
}
