using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core;

public class CallResult
{
	public bool Success { get; set; }

	public List<Error> Errors { get; set; }

	public CallResult()
	{
		Errors = new List<Error>();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("CallResult - Success: {0}", Success);
		stringBuilder.AppendErrors(Errors);
		return stringBuilder.ToString();
	}

	public CallResult ErrorAndLog(string code, string description)
	{
		Errors.Add(LocalExtensions.MakeError(code, description, null));
		LogHelper.Instance.LogError(LogHelper.CallingFunction, code, description);
		return this;
	}

	public CallResult ErrorAndLog(string code, string description, Exception e)
	{
		Errors.Add(LocalExtensions.MakeError(code, description, e));
		LogHelper.Instance.LogError(LogHelper.CallingFunction, code, description);
		LogHelper.Instance.LogException(LogHelper.CallingFunction, $"{code} - {description}", e);
		return this;
	}

	public CallResult ErrorAndLog(List<Error> errors)
	{
		Errors.AddRange(errors);
		errors.ForEach(delegate(Error e)
		{
			LogHelper.Instance.LogError(LogHelper.CallingFunction, e.Code, e.Description);
		});
		return this;
	}
}
public class CallResult<T>
{
	public bool Success { get; set; }

	public T Result { get; set; }

	public List<Error> Errors { get; set; }

	public CallResult()
	{
		Errors = new List<Error>();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendFormat("CallResult - Success: {0}, Result: {1}", Success, Result);
		stringBuilder.AppendErrors(Errors);
		return stringBuilder.ToString();
	}

	public CallResult<T> ErrorAndLog(string code, string description)
	{
		Errors.Add(LocalExtensions.MakeError(code, description, null));
		LogHelper.Instance.LogError(LogHelper.CallingFunction, code, description);
		return this;
	}

	public CallResult<T> ErrorAndLog(string code, string description, Exception e)
	{
		Errors.Add(LocalExtensions.MakeError(code, description, e));
		LogHelper.Instance.LogError(LogHelper.CallingFunction, code, description);
		LogHelper.Instance.LogException(LogHelper.CallingFunction, $"{code} - {description}", e);
		return this;
	}

	public CallResult<T> ErrorAndLog(List<Error> errors)
	{
		Errors.AddRange(errors);
		errors.ForEach(delegate(Error e)
		{
			LogHelper.Instance.LogError(LogHelper.CallingFunction, e.Code, e.Description);
		});
		return this;
	}
}
