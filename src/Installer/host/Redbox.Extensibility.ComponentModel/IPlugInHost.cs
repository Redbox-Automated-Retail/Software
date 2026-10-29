using System;
using System.Collections.Specialized;

namespace Redbox.Extensibility.ComponentModel;

public interface IPlugInHost : IDisposable
{
	IPlugIn PlugIn { get; set; }

	T Invoke<T>(string actionName, params object[] parms);

	ErrorList Load();

	ErrorList Unload();

	ErrorList Activate();

	ErrorList Deactivate();

	StringDictionary GetActions();
}
