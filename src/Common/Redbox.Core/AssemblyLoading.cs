// Written for the modernize branch (not decompiled).
using System;
using System.IO;
using System.Reflection;
#if !NETFRAMEWORK
using System.Linq;
using System.Runtime.Loader;
#endif

namespace Redbox.Core;

public static class AssemblyLoading
{
	// Assembly.LoadFile as the engine relies on it: when the file is already loaded (the engine's own UI assemblies are),
	// the loaded assembly is returned, so its types are the same types. On .NET Framework that is what LoadFile did. On
	// .NET, LoadFile puts every file into a new, isolated load context (a second copy whose types do not match), so the
	// assembly already loaded in the default context is used, or the file is loaded into the default context.
	public static Assembly LoadFile(string path)
	{
#if NETFRAMEWORK
		return Assembly.LoadFile(path);
#else
		string fullPath = Path.GetFullPath(path);
		string name = AssemblyName.GetAssemblyName(fullPath).Name;
		Assembly loaded = AssemblyLoadContext.Default.Assemblies.FirstOrDefault(a => string.Equals(a.GetName().Name, name, StringComparison.OrdinalIgnoreCase));
		return loaded ?? AssemblyLoadContext.Default.LoadFromAssemblyPath(fullPath);
#endif
	}
}
