namespace Redbox.HAL.Component.Model;

public interface IEmptySearchPatternService
{
	void DumpESP(bool dumpStore);

	void AddObserver(IExcludeEmptySearchLocationObserver observer);

	void RemoveObserver(IExcludeEmptySearchLocationObserver observer);

	IEmptySearchResult FindMultipleEmptyLocations();

	IEmptySearchResult FindOneEmpty();

	IEmptySearchResult FindEmptyOutsideOf(ILocation top);
}
