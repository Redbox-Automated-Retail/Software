using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using Redbox.Core;

namespace Redbox.IPC.Framework.Remoting;

public class RemotingServerSession : ISession, Redbox.Core.IMessageSink
{
	private Action<string, string> _paramAction;

	private Action<ISession> _beforeCommandAction;

	private List<string> _filters = new List<string>();

	private bool _enableFilters;

	private ParameterDictionary m_context;

	private Dictionary<string, object> m_properties;

	public List<string> Filters => _filters;

	public bool EnableFilters
	{
		get
		{
			return _enableFilters;
		}
		set
		{
			_enableFilters = value;
		}
	}

	public ParameterDictionary Context => m_context ?? (m_context = new ParameterDictionary());

	public IDictionary<string, object> Properties => m_properties ?? (m_properties = new Dictionary<string, object>());

	public event EventHandler Disconnect;

	public bool Send(string message)
	{
		throw new NotImplementedException("Not valid for RemotingChannel");
	}

	public void Start()
	{
		throw new NotImplementedException("Not valid for RemotingChannel");
	}

	public void SetParamAction(Action<string, string> paramAction)
	{
		_paramAction = paramAction;
	}

	public void SetBeforeCommandAction(Action<ISession> beforeCommandAction)
	{
		_beforeCommandAction = beforeCommandAction;
	}

	public void SetFilters(List<string> filters)
	{
		_filters.Clear();
		filters.ForEach(_filters.Add);
	}

	public string ProcessRequest(string commandString)
	{
		if (_beforeCommandAction != null)
		{
			_beforeCommandAction(this);
		}
		string originIP = CallContext.GetData("RemoteHostIP") as string;
		CommandResult commandResult = CommandService.Instance.Execute(this, originIP, commandString, Filters, EnableFilters, _paramAction);
		if (commandResult == null)
		{
			return null;
		}
		ProtocolHelper.FormatErrors(commandResult.Errors, commandResult.Messages);
		return commandResult.ToString();
	}

	public bool IsConnected()
	{
		return true;
	}
}
