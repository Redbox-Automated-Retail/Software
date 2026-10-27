namespace Redbox.HAL.Component.Model;

public interface IExcludeEmptySearchLocationObserver
{
	bool ShouldExclude(ILocation location);
}
