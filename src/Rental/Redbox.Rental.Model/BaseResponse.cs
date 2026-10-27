using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.Model;

public class BaseResponse : IBaseResponse
{
	public bool Success { get; set; }

	public List<Error> Errors { get; set; } = new List<Error>();

	public string HostName { get; set; }

	public int StatusCode { get; set; }

	public virtual void ScrubData()
	{
	}

	public void AddError(string code, string message)
	{
		Errors.Add(new Error
		{
			Code = code,
			Message = message
		});
	}

	public override string ToString()
	{
		return this.ToJson().ToPrettyJson();
	}
}
