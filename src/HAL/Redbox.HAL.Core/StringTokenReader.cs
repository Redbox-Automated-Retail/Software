using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

public class StringTokenReader : ITokenReader, IDisposable
{
	private readonly string m_statement;

	public ushort Column { get; private set; }

	public ushort Row { get; private set; }

	public string RemainingTokens => m_statement.Substring(Column);

	public StringTokenReader(int row, string statement)
	{
		m_statement = statement;
		Row = (ushort)row;
		Column = 0;
	}

	public char GetCurrentToken()
	{
		return m_statement[Column];
	}

	public bool MoveToNextToken()
	{
		Column++;
		return Column < m_statement.Length;
	}

	public void Reset()
	{
		Column = 0;
	}

	public char? PeekNextToken(int i)
	{
		if (Column + i >= m_statement.Length)
		{
			return null;
		}
		return m_statement[Column + i];
	}

	public char? PeekNextToken()
	{
		return PeekNextToken(1);
	}

	public bool IsEmpty()
	{
		return string.IsNullOrEmpty(m_statement);
	}

	public void IncrementRowCount()
	{
		Row++;
	}

	public void Dispose()
	{
	}
}
