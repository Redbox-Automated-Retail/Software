using System;
using Redbox.Rental.Model.Personalization;

namespace Redbox.Rental.Services.Controllers;

public class PerksConfirmViewParameters
{
	public string EmailAddress { get; set; }

	public Action AcceptAction { get; set; }

	public Action CancelAction { get; set; }

	public string IdleTimerName { get; set; }

	public SignUpFlowContext SignUpFlowContext { get; set; }
}
