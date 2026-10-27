using System;
using System.Collections.Generic;
using System.IO;

namespace Redbox.HAL.Component.Model;

public interface IExecutionService
{
	bool IsImmediateJobPending { get; }

	event EventHandler<EngineModeChangeEventArgs> EngineModeChange;

	IExecutionContext ScheduleJob(string program, string label, DateTime? start, ProgramPriority priority);

	IExecutionContext GetActiveContext();

	IExecutionContext GetJob(string id);

	IExecutionContext[] GetByLabel(string label);

	string[] GetProgramList();

	List<IExecutionContext> GetJobList();

	ExecutionContextStatus GetInitJobStatus();

	void RemoveProgram(string name);

	void CleanupJobs(bool force);

	void Enter(EngineModes mode);

	void Exit(EngineModes mode);

	void Initialize(ErrorList errors);

	void Start();

	void Shutdown();

	void Suspend();

	void Restart();

	IExecutionResult CompileImmediate(Stream stream);

	IExecutionResult Compile(Stream stream, string programName);

	IExecutionContext ExecuteImmediate();

	void PushValue(IExecutionContext ctx, string value, StackEnd end, ErrorList errors);
}
