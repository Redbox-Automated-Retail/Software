using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Core;

internal sealed class RedboxSerialPortConfiguration : ICommChannelConfiguration
{
	public int? ReceiveBufferSize { get; set; }

	public byte[] WriteTerminator { get; set; }

	public int WritePause { get; set; }

	public int ReadTimeout { get; set; }

	public int WriteTimeout { get; set; }

	public int OpenPause { get; set; }

	public bool EnableDebug { get; set; }

	internal RedboxSerialPortConfiguration()
	{
		WritePause = 10;
		ReadTimeout = -1;
		WriteTimeout = 5000;
		OpenPause = 3000;
		WriteTerminator = null;
	}
}
