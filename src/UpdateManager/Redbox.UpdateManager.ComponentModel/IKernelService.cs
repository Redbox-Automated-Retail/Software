using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Redbox.UpdateManager.ComponentModel;

public interface IKernelService
{
	string StartupScript { get; set; }

	bool ScriptCompleted { get; }

	ShutdownType ShutdownType { get; set; }

	bool IsShuttingDown { get; }

	void Reset();

	string FormatLuaValue(object value);

	ReadOnlyCollection<object> ExecuteChunk(string chunk);

	ReadOnlyCollection<object> ExecuteChunkLockCheck(string chunk, out bool locked);

	ReadOnlyCollection<object> ExecuteChunk(string chunk, bool reset);

	ReadOnlyCollection<object> ExecuteChunkNoLock(string chunk, bool reset, string resultTableName, out Dictionary<object, object> result, out bool scriptCompleted);

	ReadOnlyCollection<object> ExecuteChunk(string chunk, bool reset, string resultTableName, out Dictionary<object, object> result, out bool scriptCompleted);

	void Execute(Action action);

	Dictionary<object, object> GetTable(string name);

	void Shutdown();

	void AddGlobal(string key, object value);

	void SetScriptComplete();

	void PerformShutdown();
}
