using System;

namespace Redbox.HAL.Component.Model;

public interface IChannelResponse : IDisposable
{
	bool CommOk { get; }

	ErrorCodes Error { get; }

	byte[] RawResponse { get; }

	bool ReponseValid { get; }

	int GetIndex(byte b);
}
