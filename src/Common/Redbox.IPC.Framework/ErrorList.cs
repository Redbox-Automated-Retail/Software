using System;
using System.Collections.Generic;

namespace Redbox.IPC.Framework;

public class ErrorList : List<Error>, ICloneable
{
	public int ErrorCount => FindAll((Error each) => !each.IsWarning).Count;

	public int WarningCount => FindAll((Error each) => each.IsWarning).Count;

	public static ErrorList NewFromStrings(string[] errors)
	{
		ErrorList errorList = new ErrorList();
		if (errors == null)
		{
			return errorList;
		}
		foreach (string error in errors)
		{
			errorList.Add(Error.Parse(error));
		}
		return errorList;
	}

	public bool ContainsCode(string code)
	{
		return Find((Error each) => each.Code == code) != null;
	}

	public bool ContainsError()
	{
		return Find((Error each) => !each.IsWarning) != null;
	}

	public int RemoveCode(string code)
	{
		return RemoveAll((Error each) => each.Code == code);
	}

	public object Clone()
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(this);
		return errorList;
	}
}
