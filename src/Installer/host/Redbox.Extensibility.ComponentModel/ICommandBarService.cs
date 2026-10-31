using System;

namespace Redbox.Extensibility.ComponentModel;

public interface ICommandBarService
{
	ICommandBar[] CommandBars { get; }

	ICommandBar AddCommandBar(Guid id, CommandBarType type, string name);

	ICommandBar GetCommandBar(Guid id);

	bool RemoveCommandBar(Guid id);

	void RemoveAll();

	ICommandBar[] GetCommandBars(string name);
}
