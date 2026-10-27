using System;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.Rental.UI.Views;

public class AnimatedDiscReturnViewParameter : IViewAnalyticsName
{
	public Action GotItButtonAction { get; set; }

	public Action TimeoutAction { get; set; }

	public string ViewAnalyticsName { get; set; }
}
