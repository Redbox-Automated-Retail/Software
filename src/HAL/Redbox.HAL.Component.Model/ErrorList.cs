using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Redbox.HAL.Component.Model;

[Serializable]
public sealed class ErrorList : List<Error>, ICloneable
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

	public void Dump(TextWriter writer)
	{
		writer.WriteLine(FormatList());
	}

	public void DumpToLog()
	{
		LogHelper.Instance.Log(FormatList());
	}

	public object Clone()
	{
		ErrorList errorList = new ErrorList();
		errorList.AddRange(this);
		return errorList;
	}

	private string FormatList()
	{
		StringBuilder builder = new StringBuilder();
		builder.AppendLine("-- Errors --");
		ForEach(delegate(Error e)
		{
			builder.AppendLine($" Code        : {e.Code}");
			builder.AppendLine($" Description : {e.Description}");
			builder.AppendLine($" Details     : {e.Details}");
		});
		return builder.ToString();
	}
}
