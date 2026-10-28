using System;
using System.IO;

namespace Redbox.HAL.Core;

public class StreamTokenReader : ITokenReader, IDisposable
{
	private int m_index;

	private int m_bufferSize;

	private readonly char[] m_buffer;

	private readonly StreamReader m_stream;

	private readonly int BufferSize;

	private readonly int MaxLookAhead;

	private bool m_resetColumnOnNextMove;

	private bool m_twoCharNewLine;

	public ushort Row { get; private set; }

	public ushort Column { get; private set; }

	public string RemainingTokens
	{
		get
		{
			throw new NotImplementedException("Not implementing for streams.");
		}
	}

	public StreamTokenReader(Stream stream, int maxLookAhead)
	{
		MaxLookAhead = maxLookAhead;
		BufferSize = MaxLookAhead + 1;
		m_buffer = new char[BufferSize];
		m_stream = new StreamReader(stream);
		Row = 1;
		Column = 1;
		m_bufferSize = m_stream.ReadBlock(m_buffer, 0, m_buffer.Length);
		m_index = 0;
		m_twoCharNewLine = false;
		m_resetColumnOnNextMove = false;
		AdavnceRowCount();
	}

	public StreamTokenReader(Stream stream)
		: this(stream, 1024)
	{
	}

	public char GetCurrentToken()
	{
		return m_buffer[m_index];
	}

	public char? PeekNextToken()
	{
		return PeekNextToken(1);
	}

	public char? PeekNextToken(int i)
	{
		if (m_index + i < 0)
		{
			return null;
		}
		if (i > MaxLookAhead)
		{
			throw new ArgumentException($"This StreamTokenReader can only look {MaxLookAhead} charactes ahead.");
		}
		if (m_index + i < m_bufferSize)
		{
			return m_buffer[m_index + i];
		}
		Array.Copy(m_buffer, m_index, m_buffer, 0, m_bufferSize - m_index);
		int num = m_stream.ReadBlock(m_buffer, m_bufferSize - m_index, m_index);
		m_bufferSize = num + (m_bufferSize - m_index);
		m_index = 0;
		if (i >= m_bufferSize)
		{
			return null;
		}
		return m_buffer[i];
	}

	public bool MoveToNextToken()
	{
		if (m_resetColumnOnNextMove)
		{
			Column = 1;
			m_resetColumnOnNextMove = false;
		}
		else
		{
			Column++;
		}
		m_index++;
		if (m_index < m_bufferSize)
		{
			AdavnceRowCount();
			return true;
		}
		m_index = 0;
		AdavnceRowCount();
		m_bufferSize = m_stream.ReadBlock(m_buffer, 0, m_buffer.Length);
		return m_bufferSize > 0;
	}

	public void Reset()
	{
	}

	public bool IsEmpty()
	{
		if (m_index >= m_bufferSize)
		{
			return m_stream.EndOfStream;
		}
		return false;
	}

	private void AdavnceRowCount()
	{
		if (GetCurrentToken() == '\r')
		{
			if (PeekNextToken().HasValue && PeekNextToken().Value == '\n')
			{
				m_twoCharNewLine = true;
				m_resetColumnOnNextMove = false;
			}
			else
			{
				m_resetColumnOnNextMove = true;
			}
			Row++;
		}
		else if (GetCurrentToken() == '\n')
		{
			Column = 1;
			if (m_twoCharNewLine)
			{
				m_resetColumnOnNextMove = true;
				m_twoCharNewLine = false;
			}
			else
			{
				m_resetColumnOnNextMove = true;
				Row++;
			}
		}
	}

	public void Dispose()
	{
		m_stream.Dispose();
	}
}
