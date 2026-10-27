using System;
using System.Runtime.Serialization;

namespace Redbox.Macros;

[Serializable]
public class ExpressionParseException : Exception
{
	private int _endPos = -1;

	private int _startPos = -1;

	public int StartPos => _startPos;

	public int EndPos => _endPos;

	public ExpressionParseException()
	{
	}

	public ExpressionParseException(string message)
		: base(message, null)
	{
	}

	public ExpressionParseException(string message, Exception inner)
		: base(message, inner)
	{
	}

	public ExpressionParseException(string message, int pos)
		: base(message, null)
	{
		_startPos = pos;
		_endPos = -1;
	}

	public ExpressionParseException(string message, int startPos, int endPos)
		: base(message, null)
	{
		_startPos = startPos;
		_endPos = endPos;
	}

	public ExpressionParseException(string message, int startPos, int endPos, Exception inner)
		: base(message, inner)
	{
		_startPos = startPos;
		_endPos = endPos;
	}

	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue("startPos", _startPos);
		info.AddValue("endPos", _endPos);
		base.GetObjectData(info, context);
	}

	protected ExpressionParseException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_startPos = (int)info.GetValue("startPos", typeof(int));
		_endPos = (int)info.GetValue("endPos", typeof(int));
	}
}
