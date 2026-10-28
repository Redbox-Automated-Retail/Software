using System.IO.Ports;

namespace Redbox.HAL.Core;

public sealed class RedboxSerialPortConfiguration
{
	public string PortName { get; private set; }

	public byte[] WriteTerminator { get; set; }

	public int Baud { get; set; }

	public Parity Parity { get; set; }

	public int DataBits { get; set; }

	public StopBits StopBits { get; set; }

	public RedboxSerialPort.ResponseComplete Test { get; set; }

	public RedboxSerialPortConfiguration(string portName)
	{
		PortName = portName;
		WriteTerminator = RedboxSerialPort.WriteCharsCR;
		Baud = 115200;
		Parity = Parity.None;
		DataBits = 8;
		StopBits = StopBits.One;
	}
}
