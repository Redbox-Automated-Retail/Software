using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IDocumentWindow : IDisposable
{
	Guid Id { get; }

	string Name { get; }

	bool IsActive { get; }

	Image Image { get; set; }

	string Title { get; set; }

	Control Control { get; set; }

	bool AllowClose { get; set; }

	event EventHandler<EventArgs> Opened;

	event EventHandler<EventArgs> Closed;

	event EventHandler<EventArgs> Closing;

	event EventHandler<EventArgs> Activated;

	event EventHandler<EventArgs> Deactivated;

	void Open();

	void Close();

	void Activate();

	void Deactivate();
}
