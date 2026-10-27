using System;
using System.Collections.Generic;
using Redbox.Core;

namespace Redbox.IPC.Framework;

public interface ISession : IMessageSink
{
	bool EnableFilters { get; set; }

	ParameterDictionary Context { get; }

	IDictionary<string, object> Properties { get; }

	event EventHandler Disconnect;

	void Start();

	void SetFilters(List<string> filters);

	void SetParamAction(Action<string, string> paramAction);

	void SetBeforeCommandAction(Action<ISession> beforeCommandAction);

	bool IsConnected();
}
