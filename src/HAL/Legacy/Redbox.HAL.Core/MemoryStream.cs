using System.IO;

namespace Redbox.HAL.Core;

internal class MemoryStream : StreamBase
{
	internal byte[] GetBuffer()
	{
		if (base.Stream != null)
		{
			return ((System.IO.MemoryStream)base.Stream).GetBuffer();
		}
		return null;
	}

	internal byte[] ToArray()
	{
		if (base.Stream != null)
		{
			return ((System.IO.MemoryStream)base.Stream).ToArray();
		}
		return null;
	}

	internal MemoryStream()
	{
		base.Stream = new System.IO.MemoryStream();
	}

	internal MemoryStream(byte[] buffer)
	{
		base.Stream = new System.IO.MemoryStream(buffer);
	}
}
