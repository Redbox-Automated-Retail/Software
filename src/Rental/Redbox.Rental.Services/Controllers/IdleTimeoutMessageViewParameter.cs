using System;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.Services.Controllers;

public class IdleTimeoutMessageViewParameter
{
	public Action YesAction { get; set; }

	public Action NoAction { get; set; }

	public string IdleTimerName { get; set; }

	internal IdleTimeoutMessageViewModel ViewModel { get; set; }
}
