using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using Redbox.HAL.Core;

namespace Redbox.HAL.IPC.Framework;

public abstract class AbstractNamedPipeServerSession
{
	private bool? m_logToConsole = null;

	public IDictionary<string, object> Properties { get; private set; }

	public ParameterDictionary Context { get; private set; }

	protected abstract string WelcomeMessage { get; }

	protected bool LogToConsole
	{
		get
		{
			if (!m_logToConsole.HasValue)
			{
				m_logToConsole = RegistryOptions.Instance.GetSecretValue<bool?>("SOFTWARE\\Redbox\\HAL", "PipeLogToConsole");
			}
			return m_logToConsole ?? false;
		}
	}

	protected bool IsSecure => false;

	private StringBuilder ReadBuilder { get; set; }

	private BasePipeChannel Channel { get; set; }

	public void Start()
	{
		Write(WelcomeMessage);
		Read();
	}

	public bool IsConnected()
	{
		return Channel.Connected;
	}

	protected AbstractNamedPipeServerSession(SafeFileHandle pipeHandle)
	{
		Channel = new ServerPipeChannel(pipeHandle);
		ReadBuilder = new StringBuilder();
		Properties = new Dictionary<string, object>();
		Context = new ParameterDictionary();
		Channel.Connected = true;
	}

	protected void Write(string s)
	{
		try
		{
			if (!s.EndsWith(Environment.NewLine))
			{
				s = $"{s}{Environment.NewLine}";
			}
			if (Channel.SendMessage(Encoding.ASCII.GetBytes(s)))
			{
				Interlocked.Add(ref Statistics.Instance.NumberOfBytesSent, s.Length);
			}
			else
			{
				LogHelper.Instance.Log("Channel.SendMessage() returned false.");
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in Write.", e);
		}
	}

	protected void Read()
	{
		bool flag = true;
		while (flag)
		{
			byte[] array = Channel.ReceiveMessage();
			if (array == null)
			{
				break;
			}
			ReadBuilder.Append(Encoding.ASCII.GetString(array, 0, array.Length));
			flag = ProcessCommand();
			ResetReadBuilder();
		}
		InternalClose();
	}

	protected abstract string ProcessClientCommand(string clientCommand);

	protected abstract void CloseSessionInner();

	private void ResetReadBuilder()
	{
		if (ReadBuilder.Length != 0)
		{
			ReadBuilder.Length = 0;
		}
	}

	private bool ProcessCommand()
	{
		string text = ReadBuilder.ToString();
		if (LogHelper.Instance.IsLevelEnabled(LogEntryType.Debug))
		{
			LogProxy("Server: received message {0}", text);
		}
		if (string.IsNullOrEmpty(text))
		{
			string text2 = $"ProcessCommand: invalid line '{text}'";
			LogHelper.Instance.Log(text2, LogEntryType.Error);
			Console.WriteLine(text2);
			return false;
		}
		if (text.StartsWith("quit", StringComparison.CurrentCultureIgnoreCase))
		{
			return false;
		}
		string text3 = ProcessClientCommand(text);
		if (string.IsNullOrEmpty(text3))
		{
			return false;
		}
		Write(text3);
		return true;
	}

	private void InternalClose()
	{
		LogProxy("Internal close on server session.");
		Channel.Dispose();
		CloseSessionInner();
	}

	private void LogProxy(string msg)
	{
		LogHelper.Instance.Log(msg);
		if (LogToConsole)
		{
			Console.WriteLine(msg);
		}
	}

	private void LogProxy(string fmt, params object[] parms)
	{
		string text = string.Format(fmt, parms);
		LogHelper.Instance.Log(text);
		if (LogToConsole)
		{
			Console.WriteLine("{0} {1}", DateTime.Now.ToShortTimeString(), text);
		}
	}
}
