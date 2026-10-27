using System;
using Redbox.KioskEngine.ComponentModel.KioskServices;

namespace Redbox.KioskEngine.Environment.KioskServices;

public class PreauthorizeResult : IPreauthorizeResult
{
	public Guid SessionId { get; set; }

	public bool IsOnline { get; set; }

	public string ServerName { get; set; }

	public bool PromptRequired { get; set; }

	public int AuthRuleId { get; set; }

	public decimal AuthAmount { get; set; }

	public int AuthDays { get; set; }

	public bool SkipAuthRule { get; set; }

	public bool StandAlone { get; set; }
}
