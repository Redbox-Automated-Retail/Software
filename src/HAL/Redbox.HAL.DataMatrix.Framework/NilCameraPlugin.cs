using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework;

internal sealed class NilCameraPlugin : ICameraPlugin
{
	public bool IsRunning => false;

	public bool SupportsReset => false;

	public bool Start()
	{
		return false;
	}

	public void Snap(string file)
	{
	}

	public bool Stop()
	{
		return false;
	}

	public void InitWithProperties(IDictionary<string, object> props)
	{
	}
}
