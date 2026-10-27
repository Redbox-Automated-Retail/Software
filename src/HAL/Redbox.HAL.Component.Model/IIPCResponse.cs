using System;

namespace Redbox.HAL.Component.Model;

public interface IIPCResponse : IDisposable
{
	bool IsComplete { get; }

	bool Accumulate(byte[] rawResponse);

	bool Accumulate(byte[] bytes, int start, int length);

	void Clear();
}
