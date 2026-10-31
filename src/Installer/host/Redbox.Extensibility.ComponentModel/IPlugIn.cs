using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Redbox.Extensibility.ComponentModel;

public interface IPlugIn : IDisposable
{
	Guid ID { get; }

	string Path { get; }

	string Author { get; }

	string Version { get; }

	string Category { get; }

	string Copyright { get; }

	string Trademark { get; }

	string Name { get; set; }

	Bitmap Icon { get; set; }

	string Title { get; set; }

	string Description { get; }

	IPlugInHost Host { get; set; }

	bool IsUnloadSupported { get; }

	ReadOnlyCollection<Guid> Dependencies { get; }
}
