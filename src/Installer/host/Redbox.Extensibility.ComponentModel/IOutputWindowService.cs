using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IOutputWindowService
{
	Control Host { get; }

	IOutputGroup AddGroup(string name);

	IOutputGroup GetGroup(string name);

	void RemoveGroup(string name);

	void SelectGroup(string name);

	void RemoveAll();
}
