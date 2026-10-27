using System;
using System.Collections.Generic;

namespace DeviceService.ComponentModel.Responses;

public abstract class ResponseBaseModel
{
	private List<Error> m_errors = new List<Error>();

	public TimeSpan TimeTaken { get; set; }

	public ResponseStatus Status { get; set; }

	public IEnumerable<Error> Errors => m_errors;

	public ResponseBaseModel()
	{
	}

	public ResponseBaseModel(IEnumerable<Error> errors)
	{
		AddErrors(errors);
	}

	public void AddError(string code, string message)
	{
		AddError(new Error
		{
			Code = code,
			Message = message
		});
	}

	public void AddError(Error error)
	{
		m_errors.Add(error);
	}

	public void AddErrors(IEnumerable<Error> errors)
	{
		m_errors.AddRange(errors);
	}

	public virtual void ObfuscateSensitiveData()
	{
	}
}
