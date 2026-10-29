using System;
using System.Collections.Generic;
using System.Text;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002ECallResult
{
	public bool Success { get; set; }

	public List<CallResultError> Errors { get; private set; }

	public Redbox_002EMacros1309_002ECallResult()
	{
		Errors = new List<CallResultError>();
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendFormat("CallResult - Success: {0}", Success);
		if (Errors.Count == 0)
		{
			return sb.ToString();
		}
		sb.Append(", Errors:");
		Errors.ForEach(delegate(CallResultError e)
		{
			sb.AppendFormat(" Code: {0}, Description: {1}", e.Code, e.Description);
			if (e.Exception != null)
			{
				sb.AppendFormat(", Exception: {0}, {1}", e.Exception.GetType(), e.Exception.Message);
			}
		});
		return sb.ToString();
	}

	public Redbox_002EMacros1309_002ECallResult ErrorAndLog(string code, string description)
	{
		CallResultError callResultError = new CallResultError();
		callResultError.Code = code;
		callResultError.Description = description;
		CallResultError item = callResultError;
		Errors.Add(item);
		Redbox_002EMacros1309_002ELogHelper.Instance.LogError(Redbox_002EMacros1309_002ELogHelper.CallingFunction, code, description);
		return this;
	}

	public Redbox_002EMacros1309_002ECallResult ErrorAndLog(string code, string description, Exception e)
	{
		CallResultError callResultError = new CallResultError();
		callResultError.Code = code;
		callResultError.Description = description;
		callResultError.Exception = e;
		CallResultError item = callResultError;
		Errors.Add(item);
		Redbox_002EMacros1309_002ELogHelper.Instance.LogError(Redbox_002EMacros1309_002ELogHelper.CallingFunction, code, description);
		Redbox_002EMacros1309_002ELogHelper.Instance.LogException(Redbox_002EMacros1309_002ELogHelper.CallingFunction, $"{code} - {description}", e);
		return this;
	}

	public Redbox_002EMacros1309_002ECallResult ErrorAndLog(List<CallResultError> errors)
	{
		Errors.AddRange(errors);
		errors.ForEach(delegate(CallResultError e)
		{
			Redbox_002EMacros1309_002ELogHelper.Instance.LogError(Redbox_002EMacros1309_002ELogHelper.CallingFunction, e.Code, e.Description);
		});
		return this;
	}
}
internal class Redbox_002EMacros1309_002ECallResult<T>
{
	public bool Success { get; set; }

	public T Result { get; set; }

	public List<CallResultError> Errors { get; private set; }

	public Redbox_002EMacros1309_002ECallResult()
	{
		Errors = new List<CallResultError>();
	}

	public override string ToString()
	{
		StringBuilder sb = new StringBuilder();
		sb.AppendFormat("CallResult - Success: {0}, Result: {1}", Success, Result);
		if (Errors.Count == 0)
		{
			return sb.ToString();
		}
		Errors.ForEach(delegate(CallResultError e)
		{
			sb.AppendFormat(" Code: {0}, Description: {1}", e.Code, e.Description);
			if (e.Exception != null)
			{
				sb.AppendFormat(", Exception: {0}, {1}", e.Exception.GetType(), e.Exception.Message);
			}
		});
		return sb.ToString();
	}

	public Redbox_002EMacros1309_002ECallResult<T> ErrorAndLog(string code, string description)
	{
		CallResultError callResultError = new CallResultError();
		callResultError.Code = code;
		callResultError.Description = description;
		CallResultError item = callResultError;
		Errors.Add(item);
		Redbox_002EMacros1309_002ELogHelper.Instance.LogError(Redbox_002EMacros1309_002ELogHelper.CallingFunction, code, description);
		return this;
	}

	public Redbox_002EMacros1309_002ECallResult<T> ErrorAndLog(string code, string description, Exception e)
	{
		CallResultError callResultError = new CallResultError();
		callResultError.Code = code;
		callResultError.Description = description;
		callResultError.Exception = e;
		CallResultError item = callResultError;
		Errors.Add(item);
		Redbox_002EMacros1309_002ELogHelper.Instance.LogError(Redbox_002EMacros1309_002ELogHelper.CallingFunction, code, description);
		Redbox_002EMacros1309_002ELogHelper.Instance.LogException(Redbox_002EMacros1309_002ELogHelper.CallingFunction, $"{code} - {description}", e);
		return this;
	}

	public Redbox_002EMacros1309_002ECallResult<T> ErrorAndLog(List<CallResultError> errors)
	{
		Errors.AddRange(errors);
		errors.ForEach(delegate(CallResultError e)
		{
			Redbox_002EMacros1309_002ELogHelper.Instance.LogError(Redbox_002EMacros1309_002ELogHelper.CallingFunction, e.Code, e.Description);
		});
		return this;
	}
}
