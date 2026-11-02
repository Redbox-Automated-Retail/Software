using System;
using System.Linq;
using Redbox.Core;
using Redbox.IPC.Framework;

namespace Redbox.Shell.Remoting;

internal class KioskEngineProxy
{
	internal static string IpcHostUrl = null!; // set by KioskEngineService.Initialize

	private static ClientSession? _clientSession;

	internal static ClientSession? IPCClientSession
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
		return ExecuteCommandString("engine ping")?.Success ?? false;
	}

	internal static ClientCommandResult? ActiviateControlPanel()
	{
		return ExecuteCommandString("engine load-control-panel");
	}

	private static ClientCommandResult? ExecuteCommandString(string command)
	{
		int i = 0;
		ClientCommandResult? clientCommandResult = null;
		for (; i <= 3; i++)
		{
			try
			{
				clientCommandResult = ClientCommand<ClientCommandResult>.ExecuteCommand(IPCClientSession, disposeWhenDone: false, command);
			}
			catch (Exception e)
			{
				LogHelper.Instance.Log("Exception in KioskEngineProxy.ExecuteCommandString()!", e);
			}
			if (clientCommandResult == null || !clientCommandResult.Errors.FindAll((Redbox.IPC.Framework.Error x) => x.Code == "J777" || x.Code == "J999" || x.Code == "J001").Any())
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
