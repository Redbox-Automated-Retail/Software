using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IDocumentWindowService
{
	IDocumentWindow[] DocumentWindows { get; }

	IDocumentWindow AddDocumentWindow(Guid id, string name, string title, Image image, Control control);

	IDocumentWindow GetActiveWindow();

	bool RemoveDocumentWindow(Guid id);

	void CloseAll();

	IDocumentWindow GetDocumentWindow(Guid id);

	IDocumentWindow[] GetDocumentWindows(string name);

	void RemoveAll();
}
