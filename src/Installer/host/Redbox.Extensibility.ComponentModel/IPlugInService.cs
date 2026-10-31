using System;
using System.Collections.Generic;

namespace Redbox.Extensibility.ComponentModel;

public interface IPlugInService
{
	IPlugIn[] PlugIns { get; }

	IPlugIn GetPlugIn(Guid id);

	bool RemovePlugIn(Guid id);

	ErrorList EnumeratePlugIns(string[] paths, List<string> names);

	ErrorList LoadPlugIn(string path, out string name);

	ErrorList ActivateAllPlugIns();

	ErrorList DeactivateAllPlugIns();
}
