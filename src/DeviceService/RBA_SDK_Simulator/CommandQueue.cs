using System;
using System.Collections.Generic;
using System.Threading;

namespace RBA_SDK_Simulator;

public class CommandQueue : ICommandQueue
{
	private readonly List<SimulatorCommand> _commands = new List<SimulatorCommand>();

	private readonly AutoResetEvent _enqueued = new AutoResetEvent(initialState: false);

	public IReadOnlyList<SimulatorCommand> Pending
	{
		get
		{
			lock (_commands)
			{
				return _commands.ToArray();
			}
		}
	}

	public void Enqueue(SimulatorCommand command)
	{
		if (command == null)
		{
			throw new ArgumentNullException(nameof(command));
		}
		lock (_commands)
		{
			_commands.Add(command);
		}
		_enqueued.Set();
	}

	public bool TryTake(Predicate<SimulatorCommand> canRun, out SimulatorCommand? command)
	{
		lock (_commands)
		{
			int index = _commands.FindIndex(canRun);
			if (index < 0)
			{
				command = null;
				return false;
			}
			command = _commands[index];
			_commands.RemoveAt(index);
			return true;
		}
	}

	public bool WaitForCommand(int millisecondsTimeout)
	{
		return _enqueued.WaitOne(millisecondsTimeout);
	}

	public void Clear()
	{
		lock (_commands)
		{
			_commands.Clear();
		}
	}
}
