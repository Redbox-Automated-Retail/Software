using System;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;

namespace Redbox.KioskEngine.ComponentModel;

public class ErrorList : List<Error>, ICloneable
{
	public int ErrorCount => FindAll((Error each) => !each.IsWarning).Count;

	public int WarningCount => FindAll((Error each) => each.IsWarning).Count;

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

	public void ToLogHelper()
	{
		if (base.Count <= 0)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		using (Enumerator enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				Error current = enumerator.Current;
				stringBuilder.AppendLine(current.ToString());
				stringBuilder.AppendLine(current.Details);
			}
		}
		LogHelper.Instance.Log(stringBuilder.ToString());
	}
}
