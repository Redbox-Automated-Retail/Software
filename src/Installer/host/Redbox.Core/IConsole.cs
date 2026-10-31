using System;

namespace Redbox.Core;

public interface IConsole : IDisposable
{
	bool IsAttachedToConsole { get; }

	void Write(string value);

	void Write(string format, params object[] parms);

	void WriteLine();

	void WriteLine(object value);

	void WriteLine(string value);

	void WriteLine(string format, params object[] parms);
}
