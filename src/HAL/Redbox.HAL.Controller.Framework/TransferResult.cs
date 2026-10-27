using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class TransferResult : ITransferResult
{
	public bool ReturnedToSource { get; internal set; }

	public bool Transferred
	{
		get
		{
			if (TransferError == ErrorCodes.Success)
			{
				return Destination != null;
			}
			return false;
		}
	}

	public ErrorCodes TransferError { get; internal set; }

	public ILocation Source { get; internal set; }

	public ILocation Destination { get; internal set; }

	internal TransferResult()
	{
		TransferError = ErrorCodes.Success;
	}
}
