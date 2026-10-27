using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using Redbox.Core;
using Redbox.IPC.Framework;

namespace Redbox.UpdateManager.ServiceProxies;

public class UpdateManagerService : IDisposable
{
	private const bool TransientSessionFlag = false;

	private ClientSession m_clientSession;

	private static readonly Guid m_applicationGuid = new Guid("{FB5E48FF-4677-4cfa-8ED1-265114D7564C}");

	public string Url { get; set; }

	public int? Timeout { get; private set; }

	internal ClientSession Session
	{
		get
		{
			if (m_clientSession != null && m_clientSession.IsDisposed)
			{
				throw new ObjectDisposedException("Cannot use disposed Update Service client session.");
			}
			if (m_clientSession == null)
			{
				m_clientSession = ClientSession.GetClientSession(IPCProtocol.Parse(Url), null);
			}
			m_clientSession.Timeout = (Timeout.HasValue ? Timeout.Value : 30000);
			return m_clientSession;
		}
		set
		{
			m_clientSession = value;
		}
	}

	public static UpdateManagerService GetService(string url)
	{
		return new UpdateManagerService(url)
		{
			Timeout = 600000
		};
	}

	public static UpdateManagerService GetService(string url, int timeout)
	{
		return new UpdateManagerService(url)
		{
			Timeout = timeout
		};
	}

	public ClientCommandResult GetStoreNumber(out string number)
	{
		number = string.Empty;
		ClientCommandResult clientCommandResult = ExecuteCommandString("data get id: 'config-store-number'");
		if (clientCommandResult.Success)
		{
			number = clientCommandResult.CommandMessages[0].ToObject<string>();
		}
		return clientCommandResult;
	}

	public ClientCommandResult IsDeveloperUI(out bool isDeveloperUI)
	{
		isDeveloperUI = false;
		ClientCommandResult clientCommandResult = ExecuteCommandString("data get id: 'config-developer-mode-flag'");
		if (clientCommandResult.Success)
		{
			isDeveloperUI = clientCommandResult.CommandMessages[0].ToObject<bool>();
		}
		return clientCommandResult;
	}

	public ClientCommandResult GetInitialSubscriptionState(out bool initialSubscriptionState)
	{
		initialSubscriptionState = false;
		ClientCommandResult clientCommandResult = ExecuteCommandString("data get id: 'config-initial-subscription-state-flag'");
		if (clientCommandResult.Success)
		{
			initialSubscriptionState = clientCommandResult.CommandMessages[0].ToObject<bool>();
		}
		return clientCommandResult;
	}

	public ClientCommandResult ExportQueue(string path)
	{
		string command = $"engine export-queue path: '{path}'";
		return ExecuteCommandString(command);
	}

	public ClientCommandResult ExportSessions(string path)
	{
		string command = $"engine export-sessions path: '{path}'";
		return ExecuteCommandString(command);
	}

	public ClientCommandResult DoWork(string filter)
	{
		string command = ((!string.IsNullOrEmpty(filter)) ? $"client dowork filter:{filter}" : $"client dowork");
		return ExecuteCommandString(command);
	}

	public ClientCommandResult Poll()
	{
		string command = $"client poll";
		return ExecuteCommandString(command);
	}

	public ClientCommandResult ShutdownEngine(int timeout, int tries)
	{
		ClientCommandResult clientCommandResult = ExecuteCommandString("engine shutdown");
		if (clientCommandResult.Success)
		{
			int num = 0;
			ManualResetEvent manualResetEvent = new ManualResetEvent(initialState: false);
			manualResetEvent.WaitOne(timeout);
			manualResetEvent.Close();
			while (IsEngineRunning() && num < tries)
			{
				ManualResetEvent manualResetEvent2 = new ManualResetEvent(initialState: false);
				manualResetEvent2.WaitOne(timeout);
				manualResetEvent2.Close();
				num++;
			}
			if (IsEngineRunning())
			{
				clientCommandResult.Success = false;
				clientCommandResult.Errors.Add(Redbox.IPC.Framework.Error.NewError("T999", "The engine failed to shutdown in the time specified.", "Try to shutdown the engine again."));
			}
		}
		return clientCommandResult;
	}

	public ClientCommandResult ShutdownEngine()
	{
		return ExecuteCommandString("engine shutdown");
	}

	public ClientCommandResult LoadBundle(string bundle)
	{
		string command = $"engine load-bundle bundle: '{bundle}'";
		return ExecuteCommandString(command);
	}

	public ClientCommandResult ResetBundle()
	{
		return ExecuteCommandString("engine reset-bundle");
	}

	public ClientCommandResult ShowOfflineScreen()
	{
		return ExecuteCommandString("engine show-offline-screen");
	}

	public ClientCommandResult BringToFront()
	{
		return ExecuteCommandString("engine bring-to-front");
	}

	public ClientCommandResult Start()
	{
		return ExecuteCommandString("engine start");
	}

	public ClientCommandResult Start(string bundleName)
	{
		string command = $"engine start bundle: '{bundleName}'";
		return ExecuteCommandString(command);
	}

	public ClientCommandResult ExecuteScript(string path)
	{
		if (!File.Exists(path))
		{
			throw new FileNotFoundException(path);
		}
		string command = $"engine execute-script name: '{path}'";
		return ExecuteCommandString(command);
	}

	public void Dispose()
	{
		if (Session != null && !Session.IsDisposed)
		{
			Session.Dispose();
		}
	}

	public ClientCommandResult ExecuteCommandString(string command)
	{
		return ClientCommand<ClientCommandResult>.ExecuteCommand(Session, disposeWhenDone: false, command);
	}

	public bool IsEngineRunning()
	{
		bool createdNew;
		using (new Mutex(initiallyOwned: false, string.Format(CultureInfo.InvariantCulture, "Local\\{0}", m_applicationGuid), out createdNew))
		{
			if (createdNew)
			{
				return Process.GetProcessesByName("kioskengine").Length != 0;
			}
			return true;
		}
	}

	internal UpdateManagerService(string url)
	{
		Url = url;
	}
}
