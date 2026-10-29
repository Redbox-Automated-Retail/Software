using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IEnvironmentHostService
{
	Size Size { get; set; }

	string Caption { get; set; }

	Point Location { get; set; }

	FormWindowState WindowState { get; set; }

	event EventHandler<EventArgs> LoadLayout;

	event EventHandler<EventArgs> SaveLayout;

	void Close();

	void InitializeHost();
}
