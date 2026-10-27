namespace Redbox.HAL.Component.Model;

public interface ICommChannelConfiguration
{
	int? ReceiveBufferSize { get; set; }

	byte[] WriteTerminator { get; set; }

	int WritePause { get; set; }

	int ReadTimeout { get; set; }

	int WriteTimeout { get; set; }

	int OpenPause { get; set; }

	bool EnableDebug { get; set; }
}
