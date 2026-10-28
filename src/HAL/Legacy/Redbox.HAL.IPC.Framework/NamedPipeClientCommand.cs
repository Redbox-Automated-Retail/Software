using System;
using System.Collections.Generic;
using System.Text;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public class NamedPipeClientCommand<T> where T : ClientCommandResult, new()
{
	private IPCProtocol m_protocol;

	public string CommandText { get; internal set; }

	public TimeSpan? ExecutionTime { get; private set; }

	internal int? Timeout { get; set; }

	private NamedPipeClientSession Session { get; set; }

	private bool DisposeWhenDone { get; set; }

	public static T ExecuteCommand(IPCProtocol protocol, string command)
	{
		return new NamedPipeClientCommand<T>(protocol, command).Execute();
	}

	public static T ExecuteCommand(IPCProtocol protocol, int? timeout, string command)
	{
		return new NamedPipeClientCommand<T>(protocol, timeout, command).Execute();
	}

	public static T ExecuteCommand(NamedPipeClientSession session, bool disposeWhenDone, string command)
	{
		return new NamedPipeClientCommand<T>(session, disposeWhenDone, command).Execute();
	}

	public T Execute()
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		T result = new T
		{
			CommandText = CommandText
		};
		NamedPipeClientSession namedPipeClientSession = ((Session != null) ? Session : NamedPipeClientSession.MakeSession(m_protocol));
		try
		{
			if (Timeout.HasValue)
			{
				namedPipeClientSession.Timeout = Timeout.Value;
			}
			if (!namedPipeClientSession.IsConnected)
			{
				namedPipeClientSession.ConnectThrowOnError();
			}
			List<string> list = namedPipeClientSession.ExecuteCommand(CommandText);
			bool success = namedPipeClientSession.IsStatusOK(list);
			result.Success = success;
			if (list.Count > 0)
			{
				string statusMessage = list[list.Count - 1];
				result.StatusMessage = statusMessage;
				list.RemoveAt(list.Count - 1);
			}
			result.CommandMessages.AddRange(list);
			List<int> list2 = ConvertMessages(result);
			foreach (int item in list2)
			{
				if (item < result.CommandMessages.Count)
				{
					result.CommandMessages.RemoveAt(item);
				}
			}
		}
		catch (TimeoutException e)
		{
			OnConnectionFailure(e, result);
		}
		catch (Exception e2)
		{
			OnUnhandledException(e2, result);
		}
		finally
		{
			TimeSpan elapsed = executionTimer.Elapsed;
			result.ExecutionTime = elapsed;
			ExecutionTime = result.ExecutionTime;
			try
			{
				if (Session == null || DisposeWhenDone)
				{
					namedPipeClientSession.Dispose();
				}
			}
			catch (Exception e3)
			{
				result.Success = false;
				result.Errors.Add(Error.NewError("J999", "An unhandled exception was raised in NamedPipeClientCommand.Execute.", e3));
			}
		}
		return result;
	}

	public override string ToString()
	{
		return $"Command: {CommandText}";
	}

	internal static List<int> ConvertMessages(T result)
	{
		List<int> list = new List<int>();
		for (int i = 0; i < result.CommandMessages.Count; i++)
		{
			if (result.CommandMessages[i].StartsWith("|*"))
			{
				list.Add(i);
				StringBuilder stringBuilder = new StringBuilder();
				stringBuilder.AppendLine(result.CommandMessages[i]);
				for (int j = i + 1; j < result.CommandMessages.Count; j++)
				{
					list.Add(j);
					stringBuilder.AppendLine(result.CommandMessages[j]);
					if (result.CommandMessages[j].EndsWith("*|"))
					{
						break;
					}
				}
				stringBuilder.Replace("|*", string.Empty);
				stringBuilder.Replace("*|", string.Empty);
				string text = stringBuilder.ToString();
				string text2 = StringExtensions.ExtractCodeFromBrackets(text, "[", "]");
				if (text2 != null)
				{
					string[] array = text.Substring(text2.Length + 2).Split("|".ToCharArray());
					if (array[0].IndexOf("WARNING") != -1)
					{
						result.Errors.Add(Error.NewWarning(text2, array[0].Trim(), (array.Length == 2) ? array[1].Trim() : string.Empty));
					}
					else
					{
						result.Errors.Add(Error.NewError(text2, array[0].Trim(), (array.Length == 2) ? array[1].Trim() : string.Empty));
					}
				}
			}
			else if (result.CommandMessages[i].StartsWith("LZMA|"))
			{
				CompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA);
				result.CommandMessages[i] = Encoding.ASCII.GetString(algorithm.Decompress(StringExtensions.Base64ToBytes(result.CommandMessages[i].Substring(5))));
			}
			else if (result.CommandMessages[i].StartsWith("GZIP|"))
			{
				CompressionAlgorithm algorithm2 = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
				result.CommandMessages[i] = Encoding.ASCII.GetString(algorithm2.Decompress(StringExtensions.Base64ToBytes(result.CommandMessages[i].Substring(5))));
			}
		}
		return list;
	}

	private NamedPipeClientCommand()
	{
	}

	private NamedPipeClientCommand(IPCProtocol protocol, string command)
	{
		m_protocol = protocol;
		CommandText = command;
	}

	private NamedPipeClientCommand(IPCProtocol protocol, int? timeout, string command)
		: this(protocol, command)
	{
		Timeout = timeout;
	}

	protected NamedPipeClientCommand(NamedPipeClientSession session, bool disposeWhenDone, string command)
	{
		Session = session;
		CommandText = command;
		DisposeWhenDone = disposeWhenDone;
		if (Session != null)
		{
			m_protocol = Session.Protocol;
		}
	}

	private void OnConnectionFailure(Exception e, ClientCommandResult result)
	{
		result.Success = false;
		result.Errors.Add(Error.NewError("J001", $"Unable to connect to command service on {m_protocol.Host}:{m_protocol.Port}.", "Check that the host and port values are correct and that the command service is running on the specified host."));
	}

	private void OnUnhandledException(Exception e, ClientCommandResult result)
	{
		result.Success = false;
		result.Errors.Add(Error.NewError("J999", "An unhandled exception was raised in NamedPipeClientCommand.Execute.", e));
	}
}
