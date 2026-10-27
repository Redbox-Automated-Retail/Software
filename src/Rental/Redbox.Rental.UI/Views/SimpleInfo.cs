using System;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

public class SimpleInfo
{
	public object LockObject = new object();

	public object Obj { get; set; }

	public Action<AnalyticsTypes> AnalyticsAction { get; set; }

	public Action CancelAction { get; set; }

	public Action ContinueAction { get; set; }

	public Action OtherAction { get; set; }

	public Action<bool> CheckBoxChangedAction { get; set; }

	public SimpleModel Model { get; set; }

	public bool? UseIdleTimer { get; set; }

	public bool HasActionExecuted { get; set; }
}
