using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.KioskEngine.ComponentModel;

public interface IKernelService : IDisposable
{
	bool IsApplicationRunning { get; }

	bool IsTestPlanExecuting { get; }

	event WaitingForDebuggerAction WaitingForDebuggerAction;

	event DebuggerStopping DebuggerStopping;

	void Reset();

	string FormatLuaValue(object value);

	ReadOnlyCollection<object> ExecuteFunction(string functionName, params object[] parms);

	ReadOnlyCollection<object> ExecuteChunk(string chunk);

	ReadOnlyCollection<object> ExecuteScript(string resourceName);

	object ToLuaTable<T>(IEnumerable<T> enumerator);

	bool RaiseDebuggerOrError(Exception e, string resourceName);

	void ExecuteApplication();

	void ExecuteTestPlan(string resourceName);

	void SetDebuggingState(bool enabled);

	void SetDebuggingFullTrace(bool enabled);

	bool LoadStartupScript(string resourceName);

	object NewTable();

	void RegisterFunctions(ReadOnlyCollection<KernelFunctionInfo> functionInfos);

	void LogLuaFunctions();
}
