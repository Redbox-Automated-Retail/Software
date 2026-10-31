using System;

namespace Redbox.Core;

internal class CallResultError
{
	public string Code { get; set; }

	public string Description { get; set; }

	public Exception Exception { get; set; }

	public override string ToString()
	{
		return $"{Code} - {Description}{((Exception == null) ? string.Empty : $": {Exception.GetType()} - {Exception.Message}")}";
	}
}
