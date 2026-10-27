using System;

namespace Redbox.Rental.UI.Views;

public class PleaseWaitParameters
{
	public Action CancelAction { get; set; }

	public Action<object> CallbackAction { get; set; }

	public string MessageText { get; set; }
}
