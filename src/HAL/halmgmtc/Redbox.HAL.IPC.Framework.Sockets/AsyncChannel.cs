using System;
using System.IO;
using System.Threading;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.IPC.Framework.Sockets;

internal sealed class AsyncChannel : IDisposable, IIPCChannel
{
	private bool Disposed;

	private readonly string Identifier;

	private readonly bool LogDetails;

	private readonly Stream Stream;

	private readonly byte[] ReadBuffer;

	private readonly AutoResetEvent ReadEvent = new AutoResetEvent(initialState: false);

	public bool IsConnected { get; private set; }

	public void Dispose()
	{
		if (!Disposed)
		{
			if (LogDetails)
			{
				LogHelper.Instance.Log("[AsyncChannel-{0}] Channel disposed.", Identifier);
			}
			Disposed = true;
			Disconnect();
		}
	}

	public byte[] Read()
	{
		return Read(30000);
	}

	public byte[] Read(int timeout)
	{
		ByteAccumulator byteAccumulator = new ByteAccumulator();
		Read(byteAccumulator, timeout);
		if (!byteAccumulator.IsComplete)
		{
			return new byte[0];
		}
		return byteAccumulator.Accumulator.ToArray();
	}

	public void Read(IIPCResponse response)
	{
		Read(response, 30000);
	}

	public void Read(IIPCResponse response, int timeout)
	{
		OnRead(response);
		if (!ReadEvent.WaitOne(timeout))
		{
			response.Clear();
		}
	}

	public bool Write(byte[] bytes)
	{
		if (!Stream.CanWrite)
		{
			return false;
		}
		try
		{
			Stream.Write(bytes, 0, bytes.Length);
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[AsyncChannel] There was an unhandled exception during write.", e);
			return false;
		}
	}

	public bool Connect()
	{
		return IsConnected = true;
	}

	public bool Disconnect()
	{
		if (IsConnected)
		{
			Stream.Dispose();
			ReadEvent.Close();
			IsConnected = false;
		}
		return true;
	}

	internal AsyncChannel(Stream stream, int bufferSize, string id)
		: this(stream, bufferSize, id, logDetails: false)
	{
	}

	internal AsyncChannel(Stream stream, int bufferSize, string id, bool logDetails)
	{
		Stream = stream;
		ReadBuffer = new byte[bufferSize];
		LogDetails = logDetails;
		Identifier = id;
	}

	private void OnRead(IIPCResponse response)
	{
		try
		{
			Stream.BeginRead(ReadBuffer, 0, ReadBuffer.Length, EndReadCallback, response);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[AsyncChannel] Read caught an exception", e);
			SafeSet();
		}
	}

	private void EndReadCallback(IAsyncResult result)
	{
		try
		{
			int num = Stream.EndRead(result);
			IIPCResponse iIPCResponse = result.AsyncState as IIPCResponse;
			if (num == 0)
			{
				SafeSet();
				return;
			}
			Interlocked.Add(ref Statistics.Instance.NumberOfBytesReceived, num);
			if (!iIPCResponse.Accumulate(ReadBuffer, 0, num))
			{
				OnRead(iIPCResponse);
			}
			else
			{
				SafeSet();
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[AsyncChannel -{Identifier}] EndReadCallback has caught an unhandled exception.", e);
			SafeSet();
		}
	}

	private bool SafeSet()
	{
		try
		{
			ReadEvent.Set();
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("[AsyncChannel] Exception caught during event set.", e);
			return false;
		}
	}
}
