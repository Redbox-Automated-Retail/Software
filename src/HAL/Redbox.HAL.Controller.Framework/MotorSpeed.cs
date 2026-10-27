using System.ComponentModel;
using System.Xml;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework;

public sealed class MotorSpeed
{
	[DisplayName("Low Speed")]
	[Description("The low (start) speed of the motor specified in pulses per second.")]
	public int Low { get; private set; }

	[DisplayName("High Speed")]
	[Description("The high (target) speed of the motor specified in pulses per second.")]
	public int High { get; private set; }

	[Browsable(false)]
	public MotorGear Gear { get; private set; }

	[DisplayName("Acceleration Time (in milliseconds)")]
	[Description("The amount of time in milliseconds between acceleration from the low speed to the high speed.")]
	public int AccelerationTime { get; private set; }

	public MotorSpeed(int low, int high, int accelerationTime, MotorGear gear)
	{
		Low = low;
		High = high;
		Gear = gear;
		AccelerationTime = accelerationTime;
	}

	public override string ToString()
	{
		return "(Motor Speed Properties)";
	}

	public static MotorSpeed FromXmlNode(XmlNode node, MotorGear gear)
	{
		int nodeValue = node.SelectSingleNode("Low").GetNodeValue(500);
		int nodeValue2 = node.SelectSingleNode("High").GetNodeValue(3000);
		int nodeValue3 = node.SelectSingleNode("AccelerationTime").GetNodeValue(300);
		return new MotorSpeed(nodeValue, nodeValue2, nodeValue3, gear);
	}

	public void SaveToXml(XmlNode node)
	{
		if (node != null)
		{
			node.SelectSingleNodeAndSetValue("Low", Low);
			node.SelectSingleNodeAndSetValue("High", High);
			node.SelectSingleNodeAndSetValue("AccelerationTime", AccelerationTime);
		}
	}

	public string GetHighSpeedCommand()
	{
		return $"HSPD {(double)High * Gear.GetStepRatio()}";
	}

	public string GetLowSpeedCommand()
	{
		return $"LSPD {(double)Low * Gear.GetStepRatio()}";
	}

	public string GetAccelerationCommand()
	{
		return $"ACCEL {AccelerationTime}";
	}
}
