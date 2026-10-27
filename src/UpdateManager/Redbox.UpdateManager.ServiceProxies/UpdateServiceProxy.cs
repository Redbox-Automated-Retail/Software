using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class UpdateServiceProxy : IUpdateService
{
	private string m_url;

	private TimeSpan m_timeout = TimeSpan.FromSeconds(120.0);

	public static UpdateServiceProxy Instance => Singleton<UpdateServiceProxy>.Instance;

	public void Initialize(string url)
	{
		m_url = url;
		ServiceLocator.Instance.AddService(typeof(IUpdateService), this);
	}

	public void Initialize(string url, TimeSpan timeout)
	{
		m_url = url;
		m_timeout = timeout;
		ServiceLocator.Instance.AddService(typeof(IUpdateService), this);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList UploadFile(string path)
	{
		return NotImplemented("UploadFile");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList StartDownloads()
	{
		return NotImplemented("StartDownloads");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList FinishDownloads()
	{
		return NotImplemented("FinishDownloads");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList FinishDownload(Guid id)
	{
		return NotImplemented("FinishDownload");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ServerPoll()
	{
		return ConvertFromIPCErrors(ExecuteCommandString("client serverpoll").Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList WCFServerPoll()
	{
		return ConvertFromIPCErrors(ExecuteCommandString("client wcfserverpoll").Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Poll()
	{
		return ConvertFromIPCErrors(ExecuteCommandString("client poll").Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList DoWork(string filter)
	{
		string command = "client dowork";
		if (!string.IsNullOrEmpty(filter))
		{
			command = $"client dowork filter: '{filter}'";
		}
		return ConvertFromIPCErrors(ExecuteCommandString(command).Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList DoWork()
	{
		return DoWork(string.Empty);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ClearWorkQueue()
	{
		return NotImplemented("ClearWorkQueue");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList DeleteFromWorkQueue(string name)
	{
		return NotImplemented("DeleteFromWorkQueue");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AddStoreToRepository(string number, string name)
	{
		return NotImplemented("AddStoreToRepository");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList RemoveStoreFromRepository(string number, string name)
	{
		return NotImplemented("RemoveStoreFromRepository");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList AddGroupToRepository(string group, string name)
	{
		return NotImplemented("AddGroupToRepository");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList RemoveGroupFromRepository(string group, string name)
	{
		return NotImplemented("RemoveGroupFromRepository");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ForcePublish(string name)
	{
		return NotImplemented("ForcePublish");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList GetSubscriptionState(string name, out DateTime lastRun, out SubscriptionState state)
	{
		lastRun = DateTime.MinValue;
		state = SubscriptionState.Idle;
		return NotImplemented("GetSubscriptionState");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList StartInstaller(string repositoryHash, string frontEndVersion, out Dictionary<string, string> response)
	{
		response = null;
		return NotImplemented("StartInstaller");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList FinishInstaller(string guid, Dictionary<string, string> data)
	{
		return NotImplemented("FinishInstaller");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList FinishInstaller(string guid, string name, string value)
	{
		return NotImplemented("FinishInstaller");
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScriptFile(string path, out string result)
	{
		string command = $"client execute-scriptfile file:'{path}'";
		ClientCommandResult clientCommandResult = ExecuteCommandString(command);
		result = "";
		if (clientCommandResult.Success)
		{
			result = clientCommandResult.CommandMessages[0];
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScriptFile(string path, out string result, bool reset)
	{
		string command = $"client execute-scriptfile file:'{path}' reset:'{reset}'";
		ClientCommandResult clientCommandResult = ExecuteCommandString(command);
		result = "";
		if (clientCommandResult.Success)
		{
			result = clientCommandResult.CommandMessages[0];
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScript(string script, out string result)
	{
		string command = $"client execute-script script:'{script}'";
		ClientCommandResult clientCommandResult = ExecuteCommandString(command);
		result = "";
		if (clientCommandResult.Success)
		{
			result = clientCommandResult.CommandMessages[0];
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList ExecuteScript(string script, out string result, bool reset)
	{
		string command = $"client execute-script script:'{script}' reset:'{reset}'";
		ClientCommandResult clientCommandResult = ExecuteCommandString(command);
		result = "";
		if (clientCommandResult.Success)
		{
			result = clientCommandResult.CommandMessages[0];
		}
		return ConvertFromIPCErrors(clientCommandResult.Errors);
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList DoInCompleteWork(IEnumerable<Guid> ids)
	{
		return NotImplemented("DoInCompleteWork");
	}

	public string StoreNumber()
	{
		return string.Empty;
	}

	private ClientCommandResult ExecuteCommandString(string command)
	{
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url, (int)m_timeout.TotalMilliseconds);
		return updateManagerService.ExecuteCommandString(command);
	}

	private Redbox.UpdateManager.ComponentModel.ErrorList NotImplemented(string method)
	{
		return new Redbox.UpdateManager.ComponentModel.ErrorList { Redbox.UpdateManager.ComponentModel.Error.NewError("USP999", $"UpdateServiceProxy.{method} not implemented.", "") };
	}

	private static Redbox.UpdateManager.ComponentModel.ErrorList ConvertFromIPCErrors(List<Redbox.IPC.Framework.Error> ipc)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errors = new Redbox.UpdateManager.ComponentModel.ErrorList();
		ipc.ForEach(delegate(Redbox.IPC.Framework.Error e)
		{
			errors.Add(e.IsWarning ? Redbox.UpdateManager.ComponentModel.Error.NewWarning(e.Code, e.Description, e.Details) : Redbox.UpdateManager.ComponentModel.Error.NewError(e.Code, e.Description, e.Details));
		});
		return errors;
	}

	private UpdateServiceProxy()
	{
	}
}
