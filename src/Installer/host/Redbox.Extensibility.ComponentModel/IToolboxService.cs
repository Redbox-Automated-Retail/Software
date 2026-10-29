using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IToolboxService
{
	Control Host { get; }

	IToolboxCategory AddCategory(string name);

	IToolboxCategory GetCategory(string name);

	bool RemoveCategory(string name);

	bool RemoveCategory(IToolboxCategory category);

	IToolboxItem GetSelected();

	void SetSelected(IToolboxItem item);
}
