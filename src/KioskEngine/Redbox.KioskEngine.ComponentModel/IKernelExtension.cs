using System;
using System.Collections.ObjectModel;
using System.Drawing;

namespace Redbox.KioskEngine.ComponentModel;

public interface IKernelExtension : IDisposable
{
	Guid ID { get; }

	string Path { get; }

	string Name { get; set; }

	Bitmap Icon { get; set; }

	string Title { get; set; }

	string Author { get; }

	string Version { get; }

	string Category { get; }

	string Copyright { get; }

	string Trademark { get; }

	string Description { get; }

	bool IsUnloadSupported { get; }

	IKernelExtensionHost Host { get; set; }

	ReadOnlyCollection<Guid> Dependencies { get; }
}
