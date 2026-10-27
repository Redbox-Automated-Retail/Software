using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Redbox.HAL.Component.Model;
using Redbox.HAL.Core;
using Redbox.IPC.Framework;

namespace Redbox.HAL.IPC.Framework;

public class ClientCommand<T> where T : ClientCommandResult, new()
{
	private TextWriter m_logFile;

	private const string IPCLogsDirectory = "C:\\Program Files\\Redbox\\KioskLogs\\IPCFramework";

	private IPCProtocol m_protocol;

	public string CommandText { get; internal set; }

	public TimeSpan? ExecutionTime { get; private set; }

	internal int? Timeout { get; set; }

	private TextWriter ErrorLogFile
	{
		get
		{
			if (m_logFile == null)
			{
				if (!Directory.Exists("C:\\Program Files\\Redbox\\KioskLogs\\IPCFramework"))
				{
					try
					{
						Directory.CreateDirectory("C:\\Program Files\\Redbox\\KioskLogs\\IPCFramework");
					}
					catch
					{
					}
				}
				Process currentProcess = Process.GetCurrentProcess();
				string path = $"IPCClientErrors-{currentProcess.ProcessName}-{currentProcess.Id}.log";
				string path2 = Path.Combine("C:\\Program Files\\Redbox\\KioskLogs\\IPCFramework", path);
				try
				{
					m_logFile = new StreamWriter(File.Open(path2, FileMode.Append, FileAccess.Write, FileShare.ReadWrite));
				}
				catch
				{
					m_logFile = StreamWriter.Null;
				}
			}
			return m_logFile;
		}
	}

	public static T ExecuteCommand(IPCProtocol protocol, string command)
	{
		return new ClientCommand<T>(protocol, command).Execute();
	}

	public static T ExecuteCommand(IPCProtocol protocol, int? timeout, string command)
	{
		return new ClientCommand<T>(protocol, timeout, command).Execute();
	}

	public static T ExecuteCommand(IIpcClientSession session, string command)
	{
		return new ClientCommand<T>(command).Execute(session);
	}

	public T Execute()
	{
		using IIpcClientSession session = ClientSessionFactory.GetClientSession(m_protocol);
		return Execute(session);
	}

	public T Execute(IIpcClientSession session)
	{
		using ExecutionTimer executionTimer = new ExecutionTimer();
		T val = new T
		{
			CommandText = CommandText
		};
		try
		{
			if (Timeout.HasValue)
			{
				session.Timeout = Timeout.Value;
			}
			if (!session.IsConnected)
			{
				session.ConnectThrowOnError();
			}
			List<string> list = session.ExecuteCommand(CommandText);
			val.Success = session.IsStatusOK(list);
			if (list.Count > 0)
			{
				val.StatusMessage = list[list.Count - 1];
				list.RemoveAt(list.Count - 1);
			}
			val.CommandMessages.AddRange(list);
			foreach (int item in ConvertMessages(val))
			{
				if (item < val.CommandMessages.Count)
				{
					val.CommandMessages.RemoveAt(item);
				}
			}
		}
		catch (SocketException ex)
		{
			ErrorLogFile.WriteLine("ClientCommand.Execute() caught a socket exception; SocketErrorCode = {0}, Native = {1}", ex.SocketErrorCode, ex.NativeErrorCode);
			ErrorLogFile.Flush();
			OnConnectionFailure(ex, val);
		}
		catch (TimeoutException ex2)
		{
			ErrorLogFile.WriteLine("ClientCommand.Execute() caught a Timeout exception ( exception Message = {0} )", ex2.Message);
			ErrorLogFile.Flush();
			OnConnectionFailure(ex2, val);
		}
		catch (Exception e)
		{
			OnUnhandledException(e, val);
		}
		finally
		{
			val.ExecutionTime = executionTimer.Elapsed;
			ExecutionTime = val.ExecutionTime;
		}
		return val;
	}

	public override string ToString()
	{
		return $"Command: {CommandText}";
	}

	protected ClientCommand(IPCProtocol protocol, string command)
	{
		m_protocol = protocol;
		CommandText = command;
	}

	protected ClientCommand(IPCProtocol protocol, int? timeout, string command)
		: this(protocol, command)
	{
		Timeout = timeout;
	}

	protected ClientCommand(string command)
	{
		CommandText = command;
	}

	protected virtual void OnConnectionFailure(Exception e, ClientCommandResult result)
	{
		result.Success = false;
		result.Errors.Add(Error.NewError("J001", $"Unable to connect to command service on {m_protocol.Host}:{m_protocol.Port}.", "Check that the host and port values are correct and that the command service is running on the specified host."));
	}

	protected virtual void OnUnhandledException(Exception e, ClientCommandResult result)
	{
		result.Success = false;
		result.Errors.Add(Error.NewError("J999", "An unhandled exception was raised in ClientCommand.Execute.", e));
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
				ICompressionAlgorithm algorithm = CompressionAlgorithm.GetAlgorithm(CompressionType.LZMA);
				result.CommandMessages[i] = Encoding.ASCII.GetString(algorithm.Decompress(StringExtensions.Base64ToBytes(result.CommandMessages[i].Substring(5))));
			}
			else if (result.CommandMessages[i].StartsWith("GZIP|"))
			{
				ICompressionAlgorithm algorithm2 = CompressionAlgorithm.GetAlgorithm(CompressionType.GZip);
				result.CommandMessages[i] = Encoding.ASCII.GetString(algorithm2.Decompress(StringExtensions.Base64ToBytes(result.CommandMessages[i].Substring(5))));
			}
		}
		return list;
	}
}
