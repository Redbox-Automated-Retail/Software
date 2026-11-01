using System;
using System.Text;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal static class RedboxChannelDecorator
{
	internal static bool Write(IIPCChannel channel, string line)
	{
		if (!line.EndsWith(Environment.NewLine))
		{
			line = $"{line}{Environment.NewLine}";
		}
		byte[] bytes = Encoding.ASCII.GetBytes(line);
		bool written = channel.Write(bytes);
		if (written)
		{
			Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, bytes.Length);
		}
		return written;
	}
}
