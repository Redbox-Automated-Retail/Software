using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel.ApiService.EngineCore;

public class KioskEngineVersions
{
	public List<AssemblyInfo> Assemblies { get; set; } = new List<AssemblyInfo>();

	public List<BundleInfo> Bundles { get; set; } = new List<BundleInfo>();
}
