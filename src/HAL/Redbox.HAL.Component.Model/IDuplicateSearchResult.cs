namespace Redbox.HAL.Component.Model;

public interface IDuplicateSearchResult
{
	bool IsDuplicate { get; }

	ILocation Original { get; }
}
