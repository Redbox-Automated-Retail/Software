using System;

namespace Redbox.Extensibility.ComponentModel;

public interface ICommand
{
	int Id { get; }

	string Name { get; }

	Guid GroupId { get; }

	bool Visible { get; set; }

	bool Enabled { get; set; }

	bool Checked { get; set; }

	object SelectedItem { get; set; }

	event EventHandler<EventArgs> Invoke;

	event EventHandler<EventArgs> StateChange;

	void Execute();

	void ClearInvoke();
}
