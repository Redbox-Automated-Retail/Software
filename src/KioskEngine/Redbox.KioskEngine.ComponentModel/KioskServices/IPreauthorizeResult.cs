using System;

namespace Redbox.KioskEngine.ComponentModel.KioskServices;

public interface IPreauthorizeResult
{
	Guid SessionId { get; set; }

	bool IsOnline { get; set; }

	string ServerName { get; set; }

	bool PromptRequired { get; set; }

	int AuthRuleId { get; set; }

	decimal AuthAmount { get; set; }

	int AuthDays { get; set; }

	bool SkipAuthRule { get; set; }

	bool StandAlone { get; set; }
}
