using System;
using System.Drawing;
using System.Windows.Forms;

namespace Redbox.Extensibility.ComponentModel;

public interface ICommandBarItem : IDisposable
{
	Control Control { get; }

	ICommand Command { get; }

	Image Image { get; set; }

	string Name { get; set; }

	string Text { get; set; }

	bool Stretch { get; set; }

	bool Enabled { get; set; }

	bool Checked { get; set; }

	bool Visible { get; set; }

	string ToolTip { get; set; }

	int MinimumSize { get; set; }

	bool BeginGroup { get; set; }

	CommandBarItemType Type { get; }

	string Description { get; set; }

	string ShortcutKeys { get; set; }

	ICommandBarItemList Items { get; }

	event CommandBarItemPopupHandler BeforePopup;

	void ShowContextMenu(Control control, Point point);
}
