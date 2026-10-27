using System.IO.Ports;

namespace Redbox.HAL.Core;

public sealed class RedboxSerialPortConfiguration
{
	public int? ReceiveBufferSize { get; set; }

	public string PortName { get; internal set; }

	public byte[] WriteTerminator { get; set; }

	public int Baud { get; set; }

	public Parity Parity { get; set; }

	public int DataBits { get; set; }

	public int WritePause { get; set; }

	public StopBits StopBits { get; set; }

	public int ReadTimeout { get; set; }

	public int WriteTimeout { get; set; }

	public int OpenWait { get; set; }

	public RedboxSerialPort.ResponseComplete Test { get; set; }

	public RedboxSerialPortConfiguration()
	{
		WriteTerminator = RedboxSerialPort.WriteCharsCR;
		Baud = 115200;
		Parity = Parity.None;
		DataBits = 8;
		StopBits = StopBits.One;
		WritePause = 10;
		ReadTimeout = -1;
		WriteTimeout = 5000;
		OpenWait = 3000;
	}

	public RedboxSerialPortConfiguration(string portName)
		: this()
	{
		PortName = portName;
	}
}
