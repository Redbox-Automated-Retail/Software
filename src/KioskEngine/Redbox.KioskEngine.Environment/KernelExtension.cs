using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class KernelExtension : IKernelExtension, IDisposable
{
	private List<Guid> m_dependencies;

	public Guid ID { get; internal set; }

	public string Name { get; set; }

	public Bitmap Icon { get; set; }

	public string Title { get; set; }

	public string Path { get; internal set; }

	public string Author { get; internal set; }

	public string Version { get; internal set; }

	public string Category { get; internal set; }

	public string Copyright { get; internal set; }

	public string Trademark { get; internal set; }

	public IKernelExtensionHost Host { get; set; }

	public string Description { get; internal set; }

	public bool IsUnloadSupported { get; internal set; }

	public ReadOnlyCollection<Guid> Dependencies => InnerDependencies.AsReadOnly();

	internal List<Guid> InnerDependencies
	{
		get
		{
			if (m_dependencies == null)
			{
				m_dependencies = new List<Guid>();
			}
			return m_dependencies;
		}
	}

	public void Dispose()
	{
		if (Icon != null)
		{
			Icon.Dispose();
		}
	}

	public void RegisterCommunicationActivity(Action<object, EventArgs> action)
	{
		Host?.RegisterCommunicationActivity(action);
	}
}
