using System;
using System.Collections.Generic;

namespace Redbox.KioskEngine.ComponentModel;

public interface IBuildSetting
{
	bool IsActive { get; set; }

	DateTime? BuiltOn { get; }

	string OutputName { get; set; }

	IDictionary<string, string> Properties { get; set; }
}
