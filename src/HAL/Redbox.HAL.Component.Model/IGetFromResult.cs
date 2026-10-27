namespace Redbox.HAL.Component.Model;

public interface IGetFromResult : IGetResult
{
	ErrorCodes MoveResult { get; }
}
