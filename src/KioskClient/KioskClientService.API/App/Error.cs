using System;
using Newtonsoft.Json;

namespace KioskClientService.API.App;

public class Error
{
	[JsonProperty(Required = Required.DisallowNull)]
	public string Code { get; set; }

	[JsonProperty(Required = Required.DisallowNull)]
	public string Message { get; set; }

	public override bool Equals(object obj)
	{
		if (obj is Error)
		{
			Error error = obj as Error;
			if (error.Code.Equals(Code) && error.Message.Equals(Message))
			{
				return true;
			}
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Code, Message);
	}
}
