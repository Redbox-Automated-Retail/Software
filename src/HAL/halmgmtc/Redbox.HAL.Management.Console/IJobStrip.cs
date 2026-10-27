using System;
using System.Collections.Generic;

namespace Redbox.HAL.Management.Console;

internal interface IJobStrip
{
	List<HardwareJobWrapper> UpdateSource();

	void UpdateButtons(object sender, EventArgs e);
}
