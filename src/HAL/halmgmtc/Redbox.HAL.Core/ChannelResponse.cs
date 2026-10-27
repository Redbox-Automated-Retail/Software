using System;
using System.Collections.Generic;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class ChannelResponse : IChannelResponse, IDisposable
{
	internal readonly byte[] Buffer;

	private bool Disposed;

	private readonly List<byte> PortResponse = new List<byte>();

	private readonly EventWaitHandle ReadEvent = new EventWaitHandle(initialState: false, EventResetMode.ManualReset);

	public bool CommOk => Error == ErrorCodes.Success;

	public ErrorCodes Error { get; internal set; }

	public byte[] RawResponse => PortResponse.ToArray();

	public bool ReponseValid { get; internal set; }

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	~ChannelResponse()
	{
		Dispose(disposing: false);
	}

	public int GetIndex(byte b)
	{
		byte[] array = PortResponse.ToArray();
		if (array == null || array.Length == 0)
		{
			return -1;
		}
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] == b)
			{
				return i;
			}
		}
		return -1;
	}

	internal void Accumulate(int len)
	{
		Accumulate(Buffer, len);
	}

	internal void Accumulate(byte[] bytes, int len)
	{
		for (int i = 0; i < len; i++)
		{
			PortResponse.Add(bytes[i]);
		}
	}

	internal bool Wait(int timeout)
	{
		return ReadEvent.WaitOne(timeout);
	}

	internal void ReadEnd()
	{
		if (!Disposed)
		{
			ReadEvent.Set();
		}
	}

	internal ChannelResponse()
		: this(0)
	{
	}

	internal ChannelResponse(int bufferSize)
	{
		Error = ErrorCodes.Success;
		Buffer = ((bufferSize <= 0) ? null : new byte[bufferSize]);
	}

	private void Dispose(bool disposing)
	{
		if (!Disposed)
		{
			Disposed = true;
			if (disposing)
			{
				ReadEvent.Close();
				PortResponse.Clear();
			}
		}
	}
}
