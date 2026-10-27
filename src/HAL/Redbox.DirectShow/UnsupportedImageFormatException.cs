using System;

namespace Redbox.DirectShow;

public class UnsupportedImageFormatException : ArgumentException
{
	public UnsupportedImageFormatException()
	{
	}

	public UnsupportedImageFormatException(string message)
		: base(message)
	{
	}

	public UnsupportedImageFormatException(string message, string paramName)
		: base(message, paramName)
	{
	}
}
