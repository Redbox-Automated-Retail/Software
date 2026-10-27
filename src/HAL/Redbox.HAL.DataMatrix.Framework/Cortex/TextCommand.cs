using System.Collections.Generic;
using System.Text;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal abstract class TextCommand : AbstractCortexCommand
{
	private byte[] m_command;

	protected override byte[] CoreCommand
	{
		get
		{
			if (m_command != null)
			{
				return m_command;
			}
			List<byte> list = new List<byte>();
			try
			{
				list.AddRange(Encoding.ASCII.GetBytes(Prefix));
				if (Data == null)
				{
					list.Add(0);
				}
				else
				{
					list.Add((byte)Data.Length);
					list.AddRange(Encoding.ASCII.GetBytes(Data));
				}
				list.Add(0);
				return m_command = list.ToArray();
			}
			finally
			{
				list.Clear();
			}
		}
	}

	protected abstract string Prefix { get; }

	protected abstract string Data { get; }
}
