using System.Collections.ObjectModel;
using System.Windows.Forms;

namespace Redbox.KioskEngine.ComponentModel;

public interface IIdeService
{
	Form MainForm { get; }

	IProject CurrentProject { get; }

	ReadOnlyCollection<string> RecentProjects { get; }

	ErrorList OpenProject(string path);

	ErrorList CloseProject();

	ErrorList Build();

	ErrorList Build(string path);

	void Show();

	void Close();

	void ActivateDebugger(string resourceName, int lineNumber, string error);

	void SetDebuggerInstance(object instance);
}
