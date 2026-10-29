using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace Redbox.Extensibility.ComponentModel;

public interface ICommandBarItemList : IList<ICommandBarItem>, ICollection<ICommandBarItem>, IEnumerable<ICommandBarItem>, IEnumerable
{
	ICommandBarItem GetByName(string name);

	ICommandBarItem AddItem(CommandBarItemType type, string text);

	ICommandBarItem AddItem(CommandBarItemType type, Image image);

	ICommandBarItem AddItem(CommandBarItemType type, Image image, string toolTip);

	ICommandBarItem AddItem(ICommand command, CommandBarItemType type, Image image);

	ICommandBarItem AddItem(ICommand command, CommandBarItemType type, string text);

	ICommandBarItem AddItem(ICommand command, CommandBarItemType type, string text, Image image);

	ICommandBarItem AddItem(ICommand command, CommandBarItemType type, Image image, string toolTip);

	ICommandBarItem AddItem(ICommand command, CommandBarItemType type, string text, Image image, string toolTip);

	ICommandBarItem InsertItem(int index, CommandBarItemType type, string text);

	ICommandBarItem InsertItem(int index, CommandBarItemType type, Image image);

	ICommandBarItem InsertItem(int index, CommandBarItemType type, Image image, string toolTip);

	ICommandBarItem InsertItem(int index, ICommand command, CommandBarItemType type, Image image);

	ICommandBarItem InsertItem(int index, ICommand command, CommandBarItemType type, string text);

	ICommandBarItem InsertItem(int index, ICommand command, CommandBarItemType type, string text, Image image);

	ICommandBarItem InsertItem(int index, ICommand command, CommandBarItemType type, Image image, string toolTip);

	ICommandBarItem InsertItem(int index, ICommand command, CommandBarItemType type, string text, Image image, string toolTip);
}
