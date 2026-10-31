using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface IErrorListService
{
	Control Host { get; }

	ErrorList Errors { get; }

	void Clear();

	void RefreshList();
}
