using System;

namespace Redbox.Core;

public static class EnvironmentHelper
{
	private static byte[] m_newLineBytes;

	public static byte[] GetNewLineBytes()
	{
		if (m_newLineBytes == null)
		{
			m_newLineBytes = new byte[Environment.NewLine.Length];
			int num = 0;
			string newLine = Environment.NewLine;
			foreach (char c in newLine)
			{
				m_newLineBytes[num++] = (byte)c;
			}
		}
		return m_newLineBytes;
	}
}
