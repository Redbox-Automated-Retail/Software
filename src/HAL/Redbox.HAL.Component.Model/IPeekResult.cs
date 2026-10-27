namespace Redbox.HAL.Component.Model;

public interface IPeekResult
{
	ILocation PeekLocation { get; }

	bool TestOk { get; }

	bool IsFull { get; }

	ErrorCodes Error { get; }
}
