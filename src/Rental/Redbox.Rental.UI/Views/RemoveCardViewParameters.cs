using System;

namespace Redbox.Rental.UI.Views;

public class RemoveCardViewParameters
{
	public bool IsQuickChip { get; set; }

	public Action ContinueAction { get; set; }
}
