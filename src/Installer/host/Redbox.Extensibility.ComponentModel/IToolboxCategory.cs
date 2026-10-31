using System.Drawing;

namespace Redbox.Extensibility.ComponentModel;

public interface IToolboxCategory
{
	IToolboxItem[] Items { get; }

	string Name { get; }

	void AddMessage(string message);

	void RemoveMessage();

	IToolboxItem AddItem(string name);

	IToolboxItem AddItem(string name, Image image);

	IToolboxItem GetItem(string name);

	bool RemoveItem(string name);

	bool RemoveItem(IToolboxItem item);
}
