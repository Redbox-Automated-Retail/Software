using System;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class AsyncReadPort : RedboxSerialPort
{
	protected override PortModes Mode => PortModes.Async;

	public override bool Open()
	{
		return OpenInner();
	}

	public override IChannelResponse SendRecv(byte[] command, int readTimeout)
	{
		ChannelResponse channelResponse = new ChannelResponse(Port.ReadBufferSize);
		try
		{
			if (!Port.IsOpen)
			{
				LogHelper.Instance.Log("Port.WriteCommand: the port was closed.", LogEntryType.Error);
				channelResponse.Error = ErrorCodes.CommunicationError;
				return channelResponse;
			}
			if (base.EnableDebugging)
			{
				LogHelper.Instance.Log("Port.WriteCommand: {0}", command);
			}
			Port.Write(command, 0, command.Length);
			if (base.WriteTerminator != null)
			{
				Port.Write(base.WriteTerminator, 0, base.WriteTerminator.Length);
			}
			PerfFunctions.SpinWait(WritePause);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Port.WriteCommand() caught an exception.", e);
			channelResponse.Error = ErrorCodes.CommunicationError;
			return channelResponse;
		}
		ReadInner(channelResponse);
		try
		{
			if (!channelResponse.Wait(readTimeout))
			{
				if (string.IsNullOrEmpty(FriendlyName))
				{
					LogHelper.Instance.Log("Communication ERROR: port read timed out", LogEntryType.Error);
				}
				else
				{
					LogHelper.Instance.Log(LogEntryType.Error, "Communication ERROR ({0}): port read timed out", FriendlyName);
				}
				channelResponse.Error = ErrorCodes.CommunicationError;
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log(LogEntryType.Error, "[AsyncPort] Send/Recv caught an exception.", ex);
			channelResponse.Error = ErrorCodes.CommunicationError;
		}
		if (base.EnableDebugging)
		{
			string text = Encoding.ASCII.GetString(channelResponse.RawResponse);
			LogHelper.Instance.Log(" Read {0}, data: {1}", (!channelResponse.CommOk) ? "timed out" : "ok", text);
			LogHelper.Instance.Log("  raw response: {0}", AsString(channelResponse.RawResponse));
		}
		return channelResponse;
	}

	internal AsyncReadPort(RedboxSerialPortConfiguration conf)
		: base(conf)
	{
	}

	private void ReadInner(ChannelResponse response)
	{
		try
		{
			Port.BaseStream.BeginRead(response.Buffer, 0, response.Buffer.Length, EndReadCallback, response);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("CoreCommand: read caught an exception.", e);
		}
	}

	private void EndReadCallback(IAsyncResult result)
	{
		int num = 0;
		ChannelResponse channelResponse = null;
		try
		{
			channelResponse = (ChannelResponse)result.AsyncState;
			num = Port.BaseStream.EndRead(result);
			if (num == 0)
			{
				LogHelper.Instance.Log("SerialPort {0}: received 0 bytes in read.", Port.PortName);
				if (base.Validate(channelResponse))
				{
					channelResponse.ReponseValid = true;
				}
				channelResponse.ReadEnd();
				return;
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("Port.EndReadCallback caught an exception.", e);
			return;
		}
		channelResponse.Accumulate(num);
		if (!base.Validate(channelResponse))
		{
			ReadInner(channelResponse);
			return;
		}
		channelResponse.ReponseValid = true;
		channelResponse.ReadEnd();
	}
}
