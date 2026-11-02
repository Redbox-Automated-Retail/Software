using System;

namespace Redbox.FMA.Controllers;

public class PopupMessageViewParameters
{
	// Both are set by the code that shows the popup (object initializer and the lines after it).
	public string MessageText { get; set; } = null!;

	public Action ContinueAction { get; set; } = null!;
}
