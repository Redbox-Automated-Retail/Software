using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Client.Services;

internal sealed class ClientControllerPosition : IControllerPosition
{
	public int? XCoordinate { get; private set; }

	public int? YCoordinate { get; private set; }

	public bool ReadOk { get; private set; }

	internal ClientControllerPosition(bool ok, int? x, int? y)
	{
		ReadOk = ok;
		XCoordinate = x;
		YCoordinate = y;
	}
}
