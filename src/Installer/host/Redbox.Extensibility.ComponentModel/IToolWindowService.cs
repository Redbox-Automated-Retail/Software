using System;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IToolWindowService
{
	IToolWindow[] ToolWindows { get; }

	IToolWindow AddToolWindow(Guid id, string text, Control control);

	IToolWindow GetToolWindow(Guid id);

	bool RemoveToolWindow(Guid id);

	void RemoveAll();
}
