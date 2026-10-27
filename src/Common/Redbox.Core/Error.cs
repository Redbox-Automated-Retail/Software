using System.Text;

namespace Redbox.Core;

public class Error
{
	public string Code { get; set; }

	public string Description { get; set; }

	public string ExceptionType { get; set; }

	public string ExceptionMessage { get; set; }

	public string ExceptionStackTrace { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Code).Append(" - ").Append(Description);
		AddExceptionPart(stringBuilder, ExceptionType);
		AddExceptionPart(stringBuilder, ExceptionMessage);
		AddExceptionPart(stringBuilder, ExceptionStackTrace);
		return stringBuilder.ToString();
	}

	private static void AddExceptionPart(StringBuilder b, string s)
	{
		if (!string.IsNullOrEmpty(s))
		{
			b.Append("; ").Append(s);
		}
	}
}
