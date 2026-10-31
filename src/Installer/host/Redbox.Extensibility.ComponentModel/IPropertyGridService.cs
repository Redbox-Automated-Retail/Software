using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IPropertyGridService
{
	Control Host { get; }

	IPropertyGridCategory GetRootCategory(string name);

	IPropertyGridCategory AddRootCategory(string name);

	IPropertyGridItem[] GetProperty(string name);

	void Clear();
}
