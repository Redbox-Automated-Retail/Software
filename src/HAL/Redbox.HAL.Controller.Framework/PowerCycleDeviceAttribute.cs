using System;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class PowerCycleDeviceAttribute : Attribute
{
	public PowerCycleDevices Device { get; set; }
}
