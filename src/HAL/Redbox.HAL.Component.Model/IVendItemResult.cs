namespace Redbox.HAL.Component.Model;

public interface IVendItemResult
{
	ErrorCodes Status { get; }

	bool Presented { get; }
}
