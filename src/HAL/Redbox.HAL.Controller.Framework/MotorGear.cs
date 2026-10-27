using System.ComponentModel;
using System.Xml;
using Redbox.HAL.Component.Model.Extensions;

namespace Redbox.HAL.Controller.Framework;

public sealed class MotorGear
{
	[DisplayName("Step Ratio")]
	[Description("The ratio of the Step Resolution value.  Step Ratio is divided by 10 and then multiplied by Step Resolution to yield the multiplied Step Ratio.")]
	public double StepRatio { get; private set; }

	[DisplayName("Pulse Ratio")]
	[Description("The Pulse Ratio in pulse units.  Each Pulse Ratio unit is multiplied by 4096, then multiplied by the multiplied Step Ratio, and finally added to the Encoder Ratio value.")]
	public int PulseRatio { get; private set; }

	[DisplayName("Encoder Ratio")]
	[Description("The Encoder Ratio in encoder units.")]
	public int EncoderRatio { get; private set; }

	[DisplayName("Step Resolution")]
	[Description("The Step Resolution in encoder units.")]
	public int StepResolution { get; private set; }

	public MotorGear(double stepRatio, int pulseRatio, int encoderRatio, int stepResolution)
	{
		StepRatio = stepRatio;
		PulseRatio = pulseRatio;
		EncoderRatio = encoderRatio;
		StepResolution = stepResolution;
	}

	public override string ToString()
	{
		return "(Motor Gear Properties)";
	}

	public static MotorGear FromXmlNode(XmlNode node)
	{
		double nodeValue = node.SelectSingleNode("StepRatio").GetNodeValue(10.0);
		int nodeValue2 = node.SelectSingleNode("PulseRatio").GetNodeValue(0);
		int nodeValue3 = node.SelectSingleNode("EncoderRatio").GetNodeValue(0);
		int nodeValue4 = node.SelectSingleNode("StepResolution").GetNodeValue(4);
		return new MotorGear(nodeValue, nodeValue2, nodeValue3, nodeValue4);
	}

	public void SaveToXml(XmlNode node)
	{
		if (node != null)
		{
			node.SelectSingleNodeAndSetValue("StepRatio", StepRatio);
			node.SelectSingleNodeAndSetValue("PulseRatio", PulseRatio);
			node.SelectSingleNodeAndSetValue("EncoderRatio", EncoderRatio);
			node.SelectSingleNodeAndSetValue("StepResolution", StepResolution);
		}
	}

	public double GetStepRatio()
	{
		return (double)StepResolution * (StepRatio / 10.0);
	}

	public int GetEncoderRatio()
	{
		return PulseRatio * 4096 * (int)GetStepRatio() + EncoderRatio;
	}
}
