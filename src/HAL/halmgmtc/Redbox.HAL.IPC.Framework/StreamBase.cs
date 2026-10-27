using System;
using System.IO;

namespace Redbox.HAL.IPC.Framework;

internal abstract class StreamBase : IDisposable
{
	internal long Length
	{
		get
		{
			if (Stream != null)
			{
				return Stream.Length;
			}
			return 0L;
		}
	}

	internal long Position
	{
		get
		{
			if (Stream != null)
			{
				return Stream.Position;
			}
			return 0L;
		}
	}

	internal bool CanRead
	{
		get
		{
			if (Stream != null)
			{
				return Stream.CanRead;
			}
			return false;
		}
	}

	internal bool CanWrite
	{
		get
		{
			if (Stream != null)
			{
				return Stream.CanWrite;
			}
			return false;
		}
	}

	internal bool CanSeek
	{
		get
		{
			if (Stream != null)
			{
				return Stream.CanSeek;
			}
			return false;
		}
	}

	internal Stream Stream { get; set; }

	public static implicit operator Stream(StreamBase wrapper)
	{
		return wrapper.Stream;
	}

	public void Dispose()
	{
		if (Stream != null)
		{
			Stream.Dispose();
		}
	}

	internal static MemoryStream New()
	{
		return new MemoryStream();
	}

	internal static MemoryStream NewOn(byte[] buffer)
	{
		return new MemoryStream(buffer);
	}

	internal static FileStream CreateFile(string path)
	{
		return new FileStream(path, FileMode.Create);
	}

	internal static FileStream OpenFile(string path)
	{
		return new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
	}

	internal void Flush()
	{
		if (Stream != null)
		{
			Stream.Flush();
		}
	}

	internal void Close()
	{
		if (Stream != null)
		{
			Stream.Close();
		}
	}

	internal IAsyncResult BeginRead(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		if (Stream != null)
		{
			return Stream.BeginRead(array, offset, numBytes, userCallback, stateObject);
		}
		return null;
	}

	internal void EndRead(IAsyncResult asyncResult)
	{
		if (Stream != null)
		{
			Stream.EndRead(asyncResult);
		}
	}

	internal IAsyncResult BeginWrite(byte[] array, int offset, int numBytes, AsyncCallback userCallback, object stateObject)
	{
		if (Stream != null)
		{
			return Stream.BeginWrite(array, offset, numBytes, userCallback, stateObject);
		}
		return null;
	}

	internal long Seek(long offset, SeekOrigin origin)
	{
		if (Stream != null)
		{
			return Stream.Seek(offset, origin);
		}
		return 0L;
	}

	internal void EndWrite(IAsyncResult asyncResult)
	{
		if (Stream != null)
		{
			Stream.EndWrite(asyncResult);
		}
	}

	internal int Read(byte[] array, int offset, int count)
	{
		if (Stream != null)
		{
			return Stream.Read(array, offset, count);
		}
		return 0;
	}

	internal void Write(byte[] array, int offset, int count)
	{
		if (Stream != null)
		{
			Stream.Write(array, offset, count);
		}
	}

	internal void WriteByte(byte value)
	{
		if (Stream != null)
		{
			Stream.WriteByte(value);
		}
	}

	internal int ReadByte()
	{
		if (Stream != null)
		{
			return Stream.ReadByte();
		}
		return 0;
	}
}
