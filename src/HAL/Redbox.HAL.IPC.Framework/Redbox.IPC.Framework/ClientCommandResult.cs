using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.IPC.Framework;

public class ClientCommandResult
{
	public readonly ErrorList Errors = new ErrorList();

	public readonly List<string> CommandMessages = new List<string>();

	public bool Success { get; set; }

	// Set by ClientCommand.Execute's object initializer.
	public string CommandText { get; internal set; } = null!;

	public string? StatusMessage { get; internal set; }

	public TimeSpan ExecutionTime { get; internal set; }
}
