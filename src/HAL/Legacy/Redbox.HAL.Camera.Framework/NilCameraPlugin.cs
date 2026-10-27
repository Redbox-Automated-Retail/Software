using System.Collections.Generic;
using Redbox.HAL.Core;

namespace Redbox.HAL.Camera.Framework;

internal sealed class NilCameraPlugin : ICameraPlugin
{
	public bool IsRunning => false;

	public bool SupportsReset => false;

	public bool Start()
	{
		return false;
	}

	public string Snap()
	{
		return null;
	}

	public bool Stop()
	{
		return false;
	}

	public void TurnOff()
	{
	}

	public void InitWithProperties(IDictionary<string, object> props)
	{
	}
}
