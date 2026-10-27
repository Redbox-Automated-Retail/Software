using System.IO;
using System.Windows.Forms;

namespace Redbox.UpdateManager.ComponentModel;

public interface IInputService
{
	DialogResult AskYesNoQuestion(string title, string message);

	FileInfo LocateFile();

	void NotifyInfo(string title, string message);
}
