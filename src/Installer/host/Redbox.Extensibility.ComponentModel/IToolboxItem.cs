using System;
using System.Drawing;

namespace Redbox.Extensibility.ComponentModel;

public interface IToolboxItem
{
	string Name { get; }

	Image Image { get; }

	IToolboxCategory Category { get; }

	event EventHandler Activate;
}
