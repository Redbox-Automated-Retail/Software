using System;
using System.IO;

namespace Redbox.Core;

public class RedirectedConsole : IConsole, IDisposable
{
	private readonly StreamWriter m_writer;

	public bool IsAttachedToConsole => true;

	public RedirectedConsole(string fileName)
	{
		m_writer = File.CreateText(fileName);
		m_writer.AutoFlush = true;
	}

	public void Dispose()
	{
		if (m_writer != null)
		{
			m_writer.Close();
			m_writer.Dispose();
		}
	}

	public void Write(string value)
	{
		m_writer.Write(value);
	}

	public void WriteLine()
	{
		m_writer.WriteLine();
	}

	public void WriteLine(object value)
	{
		m_writer.WriteLine(value);
	}

	public void WriteLine(string value)
	{
		m_writer.WriteLine(value);
	}

	public void WriteLine(string format, params object[] parms)
	{
		m_writer.WriteLine(format, parms);
	}
}
