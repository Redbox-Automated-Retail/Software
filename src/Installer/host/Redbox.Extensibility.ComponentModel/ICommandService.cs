using System;

namespace Redbox.Extensibility.ComponentModel;

public interface ICommandService
{
	ICommandList Commands { get; }

	ICommand AddCommand(Guid groupId, int id, string name);

	ICommand GetCommand(Guid groupId, int id);

	ICommand[] GetCommands(string name);

	ICommand[] GetCommands(Guid groupId);

	bool RemoveCommand(Guid groupId, int id);

	void RemoveAll();

	void RemoveAll(Guid groupId);
}
