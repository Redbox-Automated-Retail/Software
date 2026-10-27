using System;
using System.Globalization;
using System.Text;
using Redbox.HAL.Component.Model;

namespace Redbox.HAL.DataMatrix.Framework.Cortex;

internal sealed class GetRegisterValueCommand : TextCommand
{
	private readonly CortexRegister Register;

	protected override string Prefix => "G";

	protected override string Data => $"({Register.Register})";

	internal bool IsCorrectlySet => OnCheckExpected();

	internal GetRegisterValueCommand(CortexRegister register)
	{
		Register = register;
	}

	private bool OnCheckExpected()
	{
		CortexPacket[] parsedPackets = base.ParsedPackets;
		if (parsedPackets.Length == 0 || parsedPackets[0].RawPacketType != 'd')
		{
			LogHelper.Instance.Log("The response packet for GetRegister is malformed ( found '{0}' packet ).", (parsedPackets.Length == 0) ? "nil" : parsedPackets[0].RawPacketType.ToString());
			return false;
		}
		int num = int.Parse(Register.Value);
		string text = Encoding.ASCII.GetString(parsedPackets[0].PayloadData, 0, 8);
		try
		{
			if (int.Parse(text, NumberStyles.HexNumber) != num)
			{
				LogHelper.Instance.Log("[GetRegisterValue] register {0} expected value {1} found {2}", Register.Register, Register.Value, text);
				return false;
			}
			return true;
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log($"[GetRegisterValue] caught an unhandled exception for register {Register} ( payload = {text} )", e);
			return false;
		}
	}
}
