using System;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class DataStoreServiceProxy : IDataStoreService
{
	private string m_url;

	public static DataStoreServiceProxy Instance => Singleton<DataStoreServiceProxy>.Instance;

	public void Initialize(string updateManagerUrl)
	{
		m_url = updateManagerUrl;
		ServiceLocator.Instance.AddService(typeof(IDataStoreService), this);
	}

	public void Set(Guid id, object o)
	{
		SetRaw(id.ToString(), o.ToJson().EscapedJson());
	}

	public void Set(string id, object o)
	{
		SetRaw(id, o.ToJson().EscapedJson());
	}

	public void SetRaw(Guid id, string o)
	{
		SetRaw(id.ToString(), o);
	}

	public void SetRaw(string id, string o)
	{
		string command = $"data set id: '{id}' o: '{o}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public T Get<T>(Guid id)
	{
		return Get<T>(id.ToString());
	}

	public T Get<T>(string id)
	{
		string raw = GetRaw(id);
		if (raw == null)
		{
			return default(T);
		}
		return raw.ToObject<T>();
	}

	public string GetRaw(Guid id)
	{
		return GetRaw(id.ToString());
	}

	public string GetRaw(string id)
	{
		string command = $"data get id: '{id}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
		if (clientCommandResult.CommandMessages.Count < 1)
		{
			return null;
		}
		return clientCommandResult.CommandMessages[0];
	}

	public void Delete(Guid id)
	{
		Delete(id.ToString());
	}

	public void Delete(string id)
	{
		string command = $"data delete id: '{id}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public void Reset()
	{
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString("data reset");
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public void CleanUp()
	{
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString("data cleanup");
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	public void CleanUp(string extension)
	{
		string command = $"data cleanup extension: '{extension}'";
		using UpdateManagerService updateManagerService = UpdateManagerService.GetService(m_url);
		ClientCommandResult clientCommandResult = updateManagerService.ExecuteCommandString(command);
		if (!clientCommandResult.Success)
		{
			string errorString = string.Empty;
			clientCommandResult.Errors.ForEach(delegate(Redbox.IPC.Framework.Error e)
			{
				errorString += $"{e} Details: {e.Details}\r\n";
			});
			throw new ApplicationException(errorString);
		}
	}

	private DataStoreServiceProxy()
	{
	}
}
