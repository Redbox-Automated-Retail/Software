using System;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface INotification
{
	string Title { get; set; }

	string Content { get; set; }

	ToolTipIcon Icon { get; set; }

	event EventHandler<EventArgs> Activated;

	void Activate();
}
