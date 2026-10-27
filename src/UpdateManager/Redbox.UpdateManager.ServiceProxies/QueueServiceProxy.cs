using System;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.ServiceProxies;

public class QueueServiceProxy : IQueueService
{
	private string m_url;

	public static QueueServiceProxy Instance => Singleton<QueueServiceProxy>.Instance;

	public void Initialize(string updateManagerUrl)
	{
		m_url = updateManagerUrl;
		ServiceLocator.Instance.AddService(typeof(IQueueService), this);
	}

	public void Enqueue(string label, object entry)
	{
		string command = $"queue enqueue label: '{label}' entry: '{entry.ToJson().EscapedJson()}'";
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

	public void EnqueueRaw(string label, string entry)
	{
		string command = $"queue enqueue label: '{label}' entry: '{entry.EscapedJson()}'";
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

	public void Dequeue(string label)
	{
		string command = $"queue dequeue label: '{label}'";
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

	public T Peek<T>(string label)
	{
		string command = $"queue peek label: '{label}'";
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
			return default(T);
		}
		return clientCommandResult.CommandMessages[0].ToObject<T>();
	}

	public string PeekRaw(string label)
	{
		string command = $"queue peek label: '{label}'";
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

	private QueueServiceProxy()
	{
	}
}
