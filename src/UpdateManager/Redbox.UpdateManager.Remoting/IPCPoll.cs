using System;
using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.IPC.Framework;
using Redbox.UpdateManager.ComponentModel;
using Redbox.UpdateService.Client;
using Redbox.UpdateService.Model;

namespace Redbox.UpdateManager.Remoting;

public class IPCPoll : IPoll
{
	private string m_storeNumber;

	private string m_url;

	private TimeSpan m_timeout;

	private int m_version;

	public static IPCPoll Instance => Singleton<IPCPoll>.Instance;

	public void Initialize(string storeNumber, string url, TimeSpan timeout, int version)
	{
		m_timeout = timeout;
		m_url = url;
		m_storeNumber = storeNumber;
		m_version = version;
	}

	public Redbox.UpdateManager.ComponentModel.ErrorList Poll(List<PollRequest> pollRequestList, out List<PollReply> pollReplyList)
	{
		Redbox.UpdateManager.ComponentModel.ErrorList errorList = new Redbox.UpdateManager.ComponentModel.ErrorList();
		pollReplyList = new List<PollReply>();
		try
		{
			if (string.IsNullOrEmpty(m_storeNumber))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WCFP01", "WCF Poll has not been initialized", "Please initialize the WCF Poll class"));
				return errorList;
			}
			if (string.IsNullOrEmpty(m_url))
			{
				errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("WCFP02", "WCF Poll has not been initialized", "Please initialize the WCF Poll class"));
				return errorList;
			}
			LogHelper.Instance.Log("Timeout for IPC poll is " + m_timeout.TotalSeconds + " seconds.", (!(m_timeout < TimeSpan.FromSeconds(30.0))) ? LogEntryType.Debug : LogEntryType.Info);
			using Redbox.UpdateService.Client.UpdateService updateService = GetService(m_url, m_timeout);
			ClientCommandResult clientCommandResult = updateService.Poll(m_storeNumber, m_version, pollRequestList, out pollReplyList);
			errorList.AddRange(IPC(clientCommandResult.Errors));
		}
		catch (Exception e)
		{
			errorList.Add(Redbox.UpdateManager.ComponentModel.Error.NewError("IPCP01", "An unhandled exception occurred while doing an IPC poll.", e));
		}
		errorList.ToLogHelper();
		return errorList;
	}

	private static Redbox.UpdateService.Client.UpdateService GetService(string url, TimeSpan timeout)
	{
		return Redbox.UpdateService.Client.UpdateService.GetService(url, Convert.ToInt32(timeout.TotalMilliseconds));
	}

	private static IEnumerable<Redbox.UpdateManager.ComponentModel.Error> IPC(IEnumerable<Redbox.IPC.Framework.Error> errors)
	{
		return errors.Select((Redbox.IPC.Framework.Error each) => (!each.IsWarning) ? Redbox.UpdateManager.ComponentModel.Error.NewError(each.Code, each.Description, each.Details) : Redbox.UpdateManager.ComponentModel.Error.NewWarning(each.Code, each.Description, each.Details));
	}

	private IPCPoll()
	{
	}
}
