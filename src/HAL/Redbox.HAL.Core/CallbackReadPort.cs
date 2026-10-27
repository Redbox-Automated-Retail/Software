using System;
using System.IO.Ports;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Threading;

namespace Redbox.HAL.Core;

internal sealed class CallbackReadPort : RedboxSerialPort
{
	private readonly AtomicFlag ReadingFlag = new AtomicFlag();

	private ChannelResponse Current;

	private byte[] ReadBuffer;

	protected override void OnPreOpenPort()
	{
		Port.DataReceived += Port_DataReceived;
		ReadBuffer = new byte[Port.ReadBufferSize];
	}

	protected override void OnPortClosed()
	{
		ReadBuffer = null;
	}

	protected override IChannelResponse OnSendReceive(byte[] command, int readTimeout)
	{
		ChannelResponse channelResponse = (Current = new ChannelResponse());
		try
		{
			try
			{
				if (!ReadingFlag.Set())
				{
					LogHelper.Instance.Log(LogEntryType.Error, "[CallbackReadPort, {0}] The port is already reading.", base.DisplayName);
					channelResponse.Error = ErrorCodes.CommunicationError;
					return channelResponse;
				}
				Port.Write(command, 0, command.Length);
				if (base.WriteTerminator != null)
				{
					Port.Write(base.WriteTerminator, 0, base.WriteTerminator.Length);
				}
				RuntimeService.SpinWait(base.WritePause);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log($"[CallbackReadPort, {base.DisplayName}] Write caught an exception.", e);
				channelResponse.Error = ErrorCodes.CommunicationError;
				return channelResponse;
			}
			if (!channelResponse.Wait(readTimeout))
			{
				channelResponse.Error = ErrorCodes.CommunicationError;
				LogHelper.Instance.Log(LogEntryType.Error, "[CallbackReadPort, {0}] Communication ERROR: port read timed out", base.DisplayName);
			}
			return channelResponse;
		}
		finally
		{
			Current = null;
			ReadingFlag.Clear();
		}
	}

	internal CallbackReadPort(SerialPort port)
		: base(port)
	{
	}

	private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
	{
		try
		{
			int bytesToRead = Port.BytesToRead;
			if (base.EnableDebugging)
			{
				LogHelper.Instance.Log("[CallbackReadPort, {0}] data received bytes = {1} args = {2}", base.DisplayName, bytesToRead, e.EventType);
			}
			if (bytesToRead == 0 || Current == null)
			{
				return;
			}
			int num = Port.Read(ReadBuffer, 0, bytesToRead);
			if (num != bytesToRead)
			{
				LogHelper.Instance.Log("[CallbackReadPort, {0}] !! The bytes to read {1} doesn't match the read count {2} !!", base.DisplayName, num, bytesToRead);
			}
			if (ReadingFlag.IsSet)
			{
				Current.Accumulate(ReadBuffer, bytesToRead);
				if (base.ValidateResponse(Current))
				{
					Current.ReadEnd();
				}
			}
		}
		catch (Exception e2)
		{
			LogHelper.Instance.Log($"[CallbackReadPort, {base.DisplayName}] data received caught an exception.", e2);
		}
	}
}
