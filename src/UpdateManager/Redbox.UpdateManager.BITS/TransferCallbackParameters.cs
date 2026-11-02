using System.Collections.Generic;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.BITS;

public class TransferCallbackParameters : ITransferCallbackParameters
{
	// Set by the caller before it is passed to SetCallback.
	public string Path { get; set; } = null!;

	public string Executable { get; set; } = null!;

	public List<string> Arguments { get; set; } = null!;
}
