using System;
using System.Collections.Generic;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Executors;

public sealed class MachineInformation : IDisposable, ICloneable<MachineInformation>
{
	public KioskConfiguration Configuration { get; internal set; }

	public List<IDeckConfig> DecksConfiguration { get; private set; }

	public void Dispose()
	{
		DecksConfiguration.Clear();
	}

	public MachineInformation Clone(params object[] parms)
	{
		MachineInformation machineInformation = new MachineInformation();
		machineInformation.Configuration = Configuration;
		machineInformation.DecksConfiguration.AddRange(DecksConfiguration);
		return machineInformation;
	}

	internal MachineInformation()
	{
		Configuration = KioskConfiguration.None;
		DecksConfiguration = new List<IDeckConfig>();
	}
}
