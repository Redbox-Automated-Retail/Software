using Redbox.HAL.Component.Model;

namespace Redbox.HAL.Controller.Framework.Services;

internal sealed class DuplicateSearchResult : IDuplicateSearchResult
{
	public bool IsDuplicate => Original != null;

	public ILocation Original { get; internal set; }
}
