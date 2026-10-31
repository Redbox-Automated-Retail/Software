using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IToolWindow : IDisposable
{
	Guid Id { get; }

	bool IsOpen { get; }

	object Tag { get; set; }

	Image Image { get; set; }

	string Text { get; set; }

	bool Visible { get; set; }

	DockStyle Dock { get; set; }

	Control Control { get; set; }

	bool AllowFloat { get; set; }

	bool AllowClose { get; set; }

	bool ShowOptions { get; set; }

	bool PersistState { get; set; }

	bool AllowDockTop { get; set; }

	bool AllowDockLeft { get; set; }

	bool AllowDockRight { get; set; }

	bool AllowDockBottom { get; set; }

	ICommandBarItem ContextMenu { get; set; }

	event EventHandler Closed;

	event EventHandler Activated;

	event EventHandler Deactivated;

	void Open();

	void OpenDocked();

	void OpenNotActivated();

	void OpenWith(IToolWindow toolWindow);

	void Activate();

	void Deactivate();

	void Hide();

	void Show();

	void Close();
}
