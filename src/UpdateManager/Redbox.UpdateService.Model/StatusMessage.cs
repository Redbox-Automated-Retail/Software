using System;
using System.Text;
using Redbox.Core;

namespace Redbox.UpdateService.Model;

public class StatusMessage
{
	public enum StatusMessageType
	{
		Info = 1,
		Error,
		Warning
	}

	public StatusMessageType Type;

	// Filled by the object initializer or the JSON reader.
	public string Key = null!;

	public string? SubKey;

	public string Description = null!;

	public DateTime TimeStamp;

	public string Data = null!;

	public bool Encode;

	public void DecodeData()
	{
		Data = Encoding.Unicode.GetString(Data.Base64ToBytes());
	}

	public void EncodeData()
	{
		Data = Encoding.Unicode.GetBytes(Data).ToBase64();
	}
}
