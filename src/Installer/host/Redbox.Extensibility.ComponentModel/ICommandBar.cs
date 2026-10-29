using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface ICommandBar : IDisposable
{
	Guid Id { get; }

	string Name { get; }

	bool IsOpen { get; }

	Size Size { get; set; }

	string Text { get; set; }

	bool Visible { get; set; }

	int DockLine { get; set; }

	int DockOffset { get; set; }

	DockStyle Dock { get; set; }

	Point Location { get; set; }

	CommandBarType Type { get; }

	ICommandBarItemList Items { get; }

	bool AllowVerticalDock { get; set; }

	bool AllowHorizontalDock { get; set; }

	event EventHandler Disposed;
}
