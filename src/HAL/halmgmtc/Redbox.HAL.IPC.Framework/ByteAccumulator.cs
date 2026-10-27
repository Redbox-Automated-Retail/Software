using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework;

internal sealed class ByteAccumulator : IIPCResponse, IDisposable
{
	internal readonly List<byte> Accumulator = new List<byte>();

	private bool Disposed;

	public bool IsComplete { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			Accumulator.Clear();
			GC.SuppressFinalize(this);
		}
	}

	public bool Accumulate(byte[] rawResponse)
	{
		return Accumulate(rawResponse, 0, rawResponse.Length);
	}

	public bool Accumulate(byte[] bytes, int start, int length)
	{
		for (int i = start; i < length; i++)
		{
			Accumulator.Add(bytes[i]);
		}
		return true;
	}

	public bool OnReadComplete()
	{
		return IsComplete = true;
	}

	public void Clear()
	{
		Accumulator.Clear();
		IsComplete = false;
	}
}
