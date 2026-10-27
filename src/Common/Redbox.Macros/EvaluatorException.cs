using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Redbox.Macros;

[Serializable]
public class EvaluatorException : ApplicationException
{
	private Location _location = Location.UnknownLocation;

	public override string Message
	{
		get
		{
			string text = base.Message;
			string text2 = string.Empty;
			if (_location != null)
			{
				text2 = _location.ToString();
			}
			if (!string.IsNullOrEmpty(text2))
			{
				text = text2 + Environment.NewLine + text;
			}
			return text;
		}
	}

	public string RawMessage => base.Message;

	public Location Location => _location;

	public EvaluatorException()
	{
	}

	public EvaluatorException(string message)
		: base(message)
	{
	}

	public EvaluatorException(string message, Exception innerException)
		: base(message, innerException)
	{
	}

	public EvaluatorException(string message, Location location)
		: base(message)
	{
		_location = location;
	}

	public EvaluatorException(string message, Location location, Exception innerException)
		: base(message, innerException)
	{
		_location = location;
	}

	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("Location", _location);
	}

	public override string ToString()
	{
		return string.Format(CultureInfo.InvariantCulture, "{0}:{1}{2}", Message, Environment.NewLine, base.ToString());
	}

	protected EvaluatorException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_location = info.GetValue("Location", _location.GetType()) as Location;
	}
}
