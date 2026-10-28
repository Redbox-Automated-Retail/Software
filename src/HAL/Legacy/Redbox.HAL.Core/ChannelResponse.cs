using System;
using System.Collections.Generic;
using System.Threading;

namespace Redbox.HAL.Core;

public sealed class ChannelResponse : IDisposable
{
	private enum ChannelError
	{
		Success,
		ChannelClosed,
		CommunicationError
	}

	public bool CommOk => Error == ChannelError.Success;

	public byte[] RawResponse => PortResponse.ToArray();

	public string CommError => Error.ToString();

	public bool ReponseValid { get; internal set; }

	public bool PortTimeout { get; private set; }

	internal AutoResetEvent ReadEvent { get; private set; }

	internal byte[] ReadBuffer { get; private set; }

	private ChannelError Error { get; set; }

	private bool Disposed { get; set; }

	private List<byte> PortResponse { get; set; }

	public static bool EndsWithEOT(ChannelResponse response)
	{
		byte[] rawResponse = response.RawResponse;
		if (rawResponse.Length >= 1)
		{
			return -1 != ByteHelper.IndexOf(rawResponse, 4);
		}
		return false;
	}

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			ReadEvent.Close();
			PortResponse.Clear();
			GC.SuppressFinalize(this);
		}
	}

	internal static ChannelResponse NewChannel()
	{
		return new ChannelResponse();
	}

	internal void SetCommunicationError()
	{
		UpdateErrorState(ChannelError.CommunicationError);
	}

	internal void SetClosedError()
	{
		UpdateErrorState(ChannelError.ChannelClosed);
	}

	internal void Accumulate(int count)
	{
		for (int i = 0; i < count; i++)
		{
			PortResponse.Add(ReadBuffer[i]);
		}
	}

	private void UpdateErrorState(ChannelError err)
	{
		Error = err;
		PortResponse.Clear();
	}

	private ChannelResponse()
	{
		Error = ChannelError.Success;
		ReadEvent = new AutoResetEvent(initialState: false);
		ReadBuffer = new byte[256];
		PortResponse = new List<byte>();
	}
}
