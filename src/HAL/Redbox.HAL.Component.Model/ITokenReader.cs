using System;

namespace Redbox.HAL.Component.Model;

public interface ITokenReader : IDisposable
{
	ushort Row { get; }

	ushort Column { get; }

	string RemainingTokens { get; }

	void Reset();

	char? PeekNextToken();

	char? PeekNextToken(int i);

	char GetCurrentToken();

	bool MoveToNextToken();

	bool IsEmpty();
}
