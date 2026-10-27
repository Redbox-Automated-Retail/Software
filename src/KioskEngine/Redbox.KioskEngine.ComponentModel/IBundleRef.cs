using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IBundleRef
{
	string Name { get; }

	string Path { get; }

	IDictionary<string, IBuildSetting> BuildSettings { get; }
}
