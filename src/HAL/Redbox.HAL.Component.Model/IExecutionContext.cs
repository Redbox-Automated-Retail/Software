using System;
using System.Collections.Generic;

namespace Redbox.HAL.Component.Model;

public interface IExecutionContext : IHardwareCorrectionObserver
{
	IFormattedLog ContextLog { get; }

	string ID { get; }

	string ProgramName { get; }

	bool IsComplete { get; }

	IMessageSink MessageSink { get; }

	bool IsImmediate { get; }

	DateTime CreatedOn { get; }

	bool IsConnected { get; }

	IExecutionResult Result { get; }

	DateTime? ExecutionStart { get; }

	DateTime? StartTime { get; set; }

	ProgramPriority Priority { get; set; }

	string Label { get; set; }

	int ResultCount { get; }

	int StackDepth { get; }

	void CreateResult(string code, string msg, string id);

	string CreateJSONResult(string code, string item, IDictionary<string, object> jsonMessage);

	void ForeachResult(Action<string> action);

	void ClearResults();

	void Trash();

	void Terminate();

	void Resume();

	void Resurrect();

	void Pend();

	void Suspend();

	bool WaitForCompletion();

	bool Connect(IMessageSink sink);

	bool Disconnect();

	ExecutionContextStatus GetStatus();

	object Pop(StackEnd end);

	void Push(object instance, StackEnd end);

	void Push(List<ILocation> locations);

	void ClearStack();

	void ForeachStackItem(Action<object> action);

	void Signal(string signal);

	void ForeachMessage(Action<string> action);

	void ClearSymbolTable();

	void ForeachSymbol(Action<IContextSymbol> action);
}
