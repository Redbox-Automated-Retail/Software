using System;
using System.Reflection;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Component.Model.Timers;

namespace Redbox.HAL.Controller.Framework;

internal sealed class CoreCommand
{
	internal readonly int OperationTimeout;

	internal readonly ICommPort Port;

	internal readonly int? StatusBit;

	internal readonly int CommandWait;

	internal readonly string ResetCommand;

	internal readonly AddressSelector Address;

	internal readonly int WaitPauseTime;

	internal readonly string CommandName;

	private static readonly char[] CommaSplit = new char[1] { ',' };

	internal string Command { get; private set; }

	internal static CoreCommand Create(CommandType type, int? timeout, ICommPort port)
	{
		return new CoreCommand(type, timeout, port);
	}

	internal static CoreCommand Create(CommandType type, ICommPort port)
	{
		return new CoreCommand(type, null, port);
	}

	internal CoreResponse Execute()
	{
		CommandTrace trace = new CommandTrace(ControllerConfiguration.Instance.EnableCommandTrace);
		try
		{
			trace.Trace("[CoreCommand] Executing command {0}", CommandName);
			CoreResponse coreResponse = SendCommand(Command, trace);
			try
			{
				if (coreResponse.CommError || !StatusBit.HasValue)
				{
					return coreResponse;
				}
				coreResponse.Error = WaitForCommand(trace);
				if (coreResponse.TimedOut && !string.IsNullOrEmpty(ResetCommand))
				{
					Array.ForEach(ResetCommand.Split(CommaSplit, StringSplitOptions.RemoveEmptyEntries), delegate(string s)
					{
						SendCommand(s, trace);
					});
				}
				return coreResponse;
			}
			finally
			{
				trace.Trace("[CoreCommand] {0} returned {1}", CommandName, coreResponse.ToString());
			}
		}
		finally
		{
			if (trace != null)
			{
				((IDisposable)trace).Dispose();
			}
		}
	}

	private ErrorCodes WaitForCommand(CommandTrace trace)
	{
		IRuntimeService service = ServiceLocator.Instance.GetService<IRuntimeService>();
		TimeSpan timespan = new TimeSpan(0, 0, 0, 0, WaitPauseTime);
		trace.Enter();
		trace.Trace("[WaitForCommand] Start.");
		try
		{
			using ExecutionTimer executionTimer = new ExecutionTimer();
			do
			{
				service.SpinWait(timespan);
				CoreResponse coreResponse = SendCommand("S", trace);
				if (coreResponse.CommError)
				{
					return ErrorCodes.CommunicationError;
				}
				trace.Trace("[WaitForCommand] {0}", coreResponse.OpCodeResponse);
				if (!coreResponse.IsBitSet(StatusBit.Value))
				{
					return ErrorCodes.Success;
				}
			}
			while (executionTimer.ElapsedMilliseconds <= OperationTimeout);
			return ErrorCodes.Timeout;
		}
		finally
		{
			trace.Exit();
		}
	}

	private CoreResponse SendCommand(string command, CommandTrace trace)
	{
		trace.Enter();
		try
		{
			CoreResponse coreResponse = new CoreResponse(Address);
			using (IChannelResponse channelResponse = Port.SendRecv(Address.ToString(), 5000))
			{
				coreResponse.Error = ((!channelResponse.CommOk) ? ErrorCodes.CommunicationError : ErrorCodes.Success);
				if (!channelResponse.CommOk)
				{
					LogHelper.Instance.Log(" [SendCommand] Selector {0} ( port = {1} )communication error.", Address.ToString(), Port.DisplayName);
					coreResponse.Diagnostic = ComputeCommunicationError(Address);
					return coreResponse;
				}
				trace.Trace("[SendCommand] Command {0} selector response {1}", command, Encoding.ASCII.GetString(channelResponse.RawResponse));
			}
			using IChannelResponse channelResponse2 = Port.SendRecv(command, CommandWait);
			coreResponse.Error = ((!channelResponse2.CommOk) ? ErrorCodes.CommunicationError : ErrorCodes.Success);
			if (channelResponse2.CommOk)
			{
				coreResponse.OpCodeResponse = Encoding.ASCII.GetString(channelResponse2.RawResponse);
				trace.Trace("[SendCommand] Command {0} response {1}", command, coreResponse.OpCodeResponse);
			}
			else
			{
				LogHelper.Instance.Log(" [SendCommand] Command {0} on address {1} ( port = {2} ) communication error.", command, Address.ToString(), Port.DisplayName);
			}
			return coreResponse;
		}
		finally
		{
			trace.Exit();
		}
	}

	private string ComputeCommunicationError(AddressSelector address)
	{
		string text = null;
		string text2 = string.Format("{0} is not responsive.", address switch
		{
			AddressSelector.H001 => "PCB", 
			AddressSelector.H002 => "AUX board", 
			AddressSelector.H101 => "SER board", 
			AddressSelector.H555 => "QR device", 
			_ => $"Unknown board {address.ToString()}", 
		});
		LogHelper.Instance.Log(text2, LogEntryType.Error);
		return text2;
	}

	private CoreCommand(CommandType type, int? timeout, ICommPort port)
	{
		Port = port;
		CommandName = type.ToString();
		FieldInfo field = typeof(CommandType).GetField(type.ToString());
		if (field == null)
		{
			LogHelper.Instance.Log("[CoreCommand] Could not locate type field on {0}", CommandName);
			return;
		}
		CommandPropertiesAttribute properties = CommandPropertiesAttribute.GetProperties(field);
		if (properties == null)
		{
			LogHelper.Instance.Log("[CoreCommand] Could not locate PropertiesAttribute on {0}", CommandName);
			return;
		}
		Command = properties.Command;
		Address = properties.Address;
		ResetCommand = properties.ResetCommand;
		WaitPauseTime = ((-1 != properties.WaitPauseTime) ? properties.WaitPauseTime : 0);
		OperationTimeout = timeout.GetValueOrDefault();
		StatusBit = null;
		if (-1 != properties.StatusBit)
		{
			StatusBit = properties.StatusBit;
			if (OperationTimeout == 0 || WaitPauseTime == 0)
			{
				LogHelper.Instance.Log("[CoreCommand] On CommandType {0} there is a status bit but Timeout = {1} and WaitPause = {2}", CommandName, OperationTimeout, WaitPauseTime);
			}
		}
		CommandWait = ((-1 != properties.CommandWait) ? properties.CommandWait : 8000);
	}
}
