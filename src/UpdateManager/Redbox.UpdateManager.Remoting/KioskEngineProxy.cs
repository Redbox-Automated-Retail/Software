using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Remoting;

public class KioskEngineProxy
{
	internal static string IpcHostUrl;

	private static ClientSession _clientSession;

	internal static ClientSession IPCClientSession
	{
		get
		{
			if (_clientSession != null && _clientSession.IsDisposed)
			{
				throw new ObjectDisposedException("Cannot use disposed Kiosk Engine client session.");
			}
			return _clientSession ?? (_clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(IpcHostUrl), 30000));
		}
		set
		{
			_clientSession = value;
		}
	}

	internal static bool IsEngineRunning()
	{
		return ExecuteCommandString("engine ping").Success;
	}

	internal static void Shutdown()
	{
		Send("engine shutdown");
	}

	internal static ClientCommandResult Send(string command)
	{
		if (!IsEngineRunning())
		{
			string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties("${engine}");
			LogHelper.Instance.Log("Engine not running. Starting engine so a message can be sent.");
			if (File.Exists(text))
			{
				if (ShellHelper.StartProcessAsShellUser(text, "--bundle:\"Rental Application,*\"") < 0)
				{
					LogHelper.Instance.Log("Error impersonating shell user. Errors found above");
				}
				ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
				manualResetEvent.WaitOne(5000);
				manualResetEvent.Close();
			}
			if (!IsEngineRunning())
			{
				LogHelper.Instance.Log("Engine did not start. Command: '{0}' not sent.", command);
				throw new ApplicationException($"Engine did not start. Command: '{command}' not sent.");
			}
		}
		LogHelper.Instance.Log("Sending IPC command '{0}' to engine.", command);
		ClientCommandResult clientCommandResult = ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (clientCommandResult.Errors.Count > 0)
			{
				foreach (Redbox.IPC.Framework.Error error in clientCommandResult.Errors)
				{
					stringBuilder.AppendLine(error.ToString());
					stringBuilder.AppendLine($"Details: {error.Details}\n");
				}
			}
			LogHelper.Instance.Log($"IPC command \"{command}\" to host \"{IpcHostUrl}\" failed.  {stringBuilder.ToString()}", LogEntryType.Error);
		}
		return clientCommandResult;
	}

	internal static ClientCommandResult ExecuteCommandString(string command)
	{
		int i = 0;
		ClientCommandResult clientCommandResult = null;
		for (; i <= 3; i++)
		{
			try
			{
				clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(IPCClientSession, disposeWhenDone: false, command);
			}
			catch (Exception ex)
			{
				Console.WriteLine("Unable to send IPC command '{0}' ({1})", command, ex.Message);
				Console.WriteLine(ex.StackTrace);
			}
			if (!clientCommandResult.Errors.FindAll((Redbox.IPC.Framework.Error x) => x.Code == "J777" || x.Code == "J999" || x.Code == "J001").Any())
			{
				break;
			}
			if (IPCClientSession != null)
			{
				IPCClientSession.Dispose();
				IPCClientSession = null;
			}
		}
		return clientCommandResult;
	}
}
