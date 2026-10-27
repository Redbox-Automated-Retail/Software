using System;

namespace Redbox.FMA.Controllers;

public class PopupMessageViewParameters
{
	public string MessageText { get; set; }

	public Action ContinueAction { get; set; }
}
