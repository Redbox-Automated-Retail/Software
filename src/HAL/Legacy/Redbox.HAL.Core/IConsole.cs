using System;

namespace Redbox.HAL.Core;

public interface IConsole : IDisposable
{
	bool IsAttachedToConsole { get; }

	void Write(string value);

	void WriteLine();

	void WriteLine(object value);

	void WriteLine(string value);

	void WriteLine(string format, params object[] parms);
}
