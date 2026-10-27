using System;
using System.Collections.Generic;

namespace KioskClientService.API.App;

public class ServiceResult
{
	public bool Success { get; set; }

	public List<Error> Errors { get; set; } = new List<Error>();

	public ServiceResult(bool success)
	{
		Success = success;
	}

	public ServiceResult(Exception exception, string errorMessage)
	{
		Success = false;
		string text = errorMessage;
		text = text + ", Exception: " + exception.Message;
		if (exception.InnerException != null)
		{
			text = text + ", Inner: " + exception.InnerException.Message;
		}
		Errors.Add(new Error
		{
			Code = "Error",
			Message = text
		});
	}
}
