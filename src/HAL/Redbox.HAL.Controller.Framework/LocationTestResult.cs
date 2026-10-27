using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework;

internal sealed class LocationTestResult : IPeekResult
{
	public bool TestOk => Error == ErrorCodes.Success;

	public bool IsFull { get; internal set; }

	public ErrorCodes Error { get; internal set; }

	public ILocation PeekLocation { get; private set; }

	internal LocationTestResult(int deck, int slot)
	{
		Error = ErrorCodes.Success;
		IInventoryService service = ServiceLocator.Instance.GetService<IInventoryService>();
		PeekLocation = service.Get(deck, slot);
	}
}
