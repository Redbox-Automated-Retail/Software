using System;

namespace Redbox.Tokenizer.Framework;

public interface ITokenReader : IDisposable
{
	int Row { get; }

	int Column { get; }

	string RemainingTokens { get; }

	void Reset();

	char? PeekNextToken();

	char? PeekNextToken(int i);

	char GetCurrentToken();

	bool MoveToNextToken();

	bool IsEmpty();
}
