using System;

namespace Redbox.HAL.Component.Model;

public interface ICommPort : IDisposable
{
	bool IsOpen { get; }

	string PortName { get; }

	bool EnableDebugging { get; set; }

	byte[] WriteTerminator { get; set; }

	int OpenPause { get; set; }

	int WritePause { get; set; }

	string DisplayName { get; set; }

	int ReadTimeout { get; set; }

	int WriteTimeout { get; set; }

	int? ReceiveBufferSize { get; set; }

	Predicate<IChannelResponse> ValidateResponse { get; set; }

	void Configure(ICommChannelConfiguration configuration);

	bool Open();

	bool Close();

	IChannelResponse SendRecv(byte[] command, int readTimeout);

	IChannelResponse SendRecv(string command, int readTimeout);
}
