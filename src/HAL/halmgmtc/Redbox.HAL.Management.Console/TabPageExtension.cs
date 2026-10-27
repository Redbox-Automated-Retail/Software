using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

public class TabPageExtension : TabPage
{
	public delegate void RefreshData();

	public RefreshData refreshData;

	public UserControl ListView { get; set; }
}
