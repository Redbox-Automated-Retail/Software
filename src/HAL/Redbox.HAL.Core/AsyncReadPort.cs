using System;
using System.IO.Ports;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class AsyncReadPort : RedboxSerialPort
{
	protected override IChannelResponse OnSendReceive(byte[] command, int readTimeout)
	{
		ChannelResponse channelResponse = new ChannelResponse(Port.ReadBufferSize);
		if (base.EnableDebugging)
		{
			LogHelper.Instance.Log("[AsyncReadPort, {0}] SendReceive response id = {1}", base.DisplayName, channelResponse.ID);
		}
		try
		{
			Port.Write(command, 0, command.Length);
			if (base.WriteTerminator != null)
			{
				Port.Write(base.WriteTerminator, 0, base.WriteTerminator.Length);
			}
			RuntimeService.SpinWait(base.WritePause);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[AsyncReadPort, {base.DisplayName}] WriteCommand caught an exception.", e);
			channelResponse.Error = ErrorCodes.CommunicationError;
			return channelResponse;
		}
		ReadInner(channelResponse);
		try
		{
			if (!channelResponse.Wait(readTimeout))
			{
				LogHelper.Instance.Log(LogEntryType.Error, "[AsyncReadPort, {0}] Communication ERROR: port read timed out", base.DisplayName);
				channelResponse.Error = ErrorCodes.CommunicationError;
			}
		}
		catch (Exception ex)
		{
			LogHelper.Instance.Log(LogEntryType.Error, $"[AsyncPort, {base.DisplayName}] Send/Recv caught an exception.", ex);
			channelResponse.Error = ErrorCodes.CommunicationError;
		}
		return channelResponse;
	}

	internal AsyncReadPort(SerialPort port)
		: base(port)
	{
	}

	private void ReadInner(ChannelResponse response)
	{
		if (base.EnableDebugging)
		{
			LogHelper.Instance.Log("[AsyncReadPort, {0}] Read inner id = {1}", base.DisplayName, response.ID);
		}
		try
		{
			Port.BaseStream.BeginRead(response.Buffer, 0, response.Buffer.Length, EndReadCallback, response);
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[AsyncReadPort, {base.DisplayName}] ReadInner caught an exception id = {response.ID}", e);
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
			if (base.EnableDebugging)
			{
				LogHelper.Instance.Log("[AsyncReadPort, {0}] EndReadCallback bytesRead = {1}, current buffer size = {2} ID  = {3}", base.DisplayName, num, channelResponse.RawResponse.Length, channelResponse.ID);
			}
			if (num == 0)
			{
				channelResponse.ReponseValid = base.ValidateResponse(channelResponse);
				channelResponse.ReadEnd();
				return;
			}
			Port.BaseStream.Flush();
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[AsyncReadPort, {base.DisplayName}] EndReadCallback caught an exception.", e);
			return;
		}
		channelResponse.Accumulate(num);
		if (!base.ValidateResponse(channelResponse))
		{
			ReadInner(channelResponse);
			return;
		}
		channelResponse.ReponseValid = true;
		channelResponse.ReadEnd();
	}
}
