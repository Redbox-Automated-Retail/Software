using System.Drawing;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IShellConfiguration
{
	Icon Icon { get; set; }

	Form HostForm { get; }

	string Title { get; set; }

	string ProductName { get; set; }

	string ProductVersion { get; set; }

	string BugReportingUrl { get; set; }

	bool ShowResizeGripper { get; set; }

	bool PersistSettings { get; set; }

	string RootRegistryPath { get; set; }

	string BugReportingArea { get; set; }

	string BugReportingProject { get; set; }

	string BugReportingUserName { get; set; }

	ErrorList Shutdown();

	ErrorList Initialize();

	void LoadLayouts();

	void SaveLayouts();

	void ThreadSafeHostUpdate(MethodInvoker invoker);
}
