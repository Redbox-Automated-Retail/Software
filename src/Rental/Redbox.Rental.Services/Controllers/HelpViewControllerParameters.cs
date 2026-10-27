using System;

namespace Redbox.Rental.Services.Controllers;

public class HelpViewControllerParameters
{
	public Action GoBackAction { get; set; }

	public Action ClickRedboxLogoAction { get; set; }

	public string AnalysisContext { get; set; }
}
