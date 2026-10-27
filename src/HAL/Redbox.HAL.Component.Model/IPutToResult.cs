namespace Redbox.HAL.Component.Model;

public interface IPutToResult : IPutResult
{
	ErrorCodes MoveResult { get; }
}
