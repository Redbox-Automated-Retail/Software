using System;
using System.IO;
using System.Text;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002EASCII85
{
	private const string PrefixMark = "<~";

	private const string SuffixMark = "~>";

	private const int m_asciiOffset = 33;

	private readonly byte[] m_encodedBlock = new byte[5];

	private readonly byte[] m_decodedBlock = new byte[4];

	private uint m_tuple;

	private int m_linePos;

	private readonly uint[] pow85 = new uint[5] { 52200625u, 614125u, 7225u, 85u, 1u };

	public static byte[] Decode(string s)
	{
		Redbox_002EMacros1309_002EASCII85 codec = new Redbox_002EMacros1309_002EASCII85();
		return codec.InternalDecode(s);
	}

	public static string Encode(byte[] source)
	{
		Redbox_002EMacros1309_002EASCII85 codec = new Redbox_002EMacros1309_002EASCII85();
		return codec.InternalEncode(source);
	}

	private byte[] InternalDecode(string s)
	{
		if (!s.StartsWith("<~") | !s.EndsWith("~>"))
		{
			throw new Exception("ASCII85 encoded data should begin with '<~' and end with '~>'");
		}
		if (s.StartsWith("<~"))
		{
			s = s.Substring("<~".Length);
		}
		if (s.EndsWith("~>"))
		{
			s = s.Substring(0, s.Length - "~>".Length);
		}
		MemoryStream memoryStream = new MemoryStream();
		int count = 0;
		bool isDataChar = false;
		string input = s;
		foreach (char c in input)
		{
			switch (c)
			{
			case 'z':
				if (count != 0)
				{
					throw new Exception("The character 'z' is invalid inside an ASCII85 block.");
				}
				m_decodedBlock[0] = 0;
				m_decodedBlock[1] = 0;
				m_decodedBlock[2] = 0;
				m_decodedBlock[3] = 0;
				memoryStream.Write(m_decodedBlock, 0, m_decodedBlock.Length);
				isDataChar = false;
				break;
			case '\0':
			case '\b':
			case '\t':
			case '\n':
			case '\f':
			case '\r':
				isDataChar = false;
				break;
			default:
				if (c < '!' || c > 'u')
				{
					throw new Exception("Bad character '" + c + "' found. ASCII85 only allows characters '!' to 'u'.");
				}
				isDataChar = true;
				break;
			}
			if (isDataChar)
			{
				m_tuple += (uint)((c - 33) * (int)pow85[count]);
				count++;
				if (count == m_encodedBlock.Length)
				{
					DecodeBlock();
					memoryStream.Write(m_decodedBlock, 0, m_decodedBlock.Length);
					m_tuple = 0u;
					count = 0;
				}
			}
		}
		switch (count)
		{
		case 1:
			throw new Exception("The last block of ASCII85 data cannot be a single byte.");
		default:
		{
			count--;
			m_tuple += pow85[count];
			DecodeBlock(count);
			for (int j = 0; j < count; j++)
			{
				memoryStream.WriteByte(m_decodedBlock[j]);
			}
			break;
		}
		case 0:
			break;
		}
		return memoryStream.ToArray();
	}

	private string InternalEncode(byte[] ba)
	{
		StringBuilder stringBuilder = new StringBuilder(ba.Length * (m_encodedBlock.Length / m_decodedBlock.Length));
		m_linePos = 0;
		AppendString(stringBuilder, "<~");
		int count = 0;
		m_tuple = 0u;
		foreach (byte b in ba)
		{
			if (count >= m_decodedBlock.Length - 1)
			{
				m_tuple |= b;
				if (m_tuple == 0)
				{
					AppendChar(stringBuilder, 'z');
				}
				else
				{
					EncodeBlock(stringBuilder);
				}
				m_tuple = 0u;
				count = 0;
			}
			else
			{
				m_tuple |= (uint)(b << 24 - count * 8);
				count++;
			}
		}
		if (count > 0)
		{
			EncodeBlock(count + 1, stringBuilder);
		}
		AppendString(stringBuilder, "~>");
		return stringBuilder.ToString();
	}

	private void EncodeBlock(StringBuilder sb)
	{
		EncodeBlock(m_encodedBlock.Length, sb);
	}

	private void EncodeBlock(int count, StringBuilder sb)
	{
		for (int i = m_encodedBlock.Length - 1; i >= 0; i--)
		{
			m_encodedBlock[i] = (byte)(m_tuple % 85 + 33);
			m_tuple /= 85u;
		}
		for (int i = 0; i < count; i++)
		{
			char c = (char)m_encodedBlock[i];
			AppendChar(sb, c);
		}
	}

	private void DecodeBlock()
	{
		DecodeBlock(m_decodedBlock.Length);
	}

	private void DecodeBlock(int bytes)
	{
		for (int i = 0; i < bytes; i++)
		{
			m_decodedBlock[i] = (byte)(m_tuple >> 24 - i * 8);
		}
	}

	private void AppendString(StringBuilder sb, string s)
	{
		m_linePos += s.Length;
		sb.Append(s);
	}

	private void AppendChar(StringBuilder sb, char c)
	{
		sb.Append(c);
		m_linePos++;
	}
}
