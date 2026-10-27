using System;

namespace Redbox.Rental.UI.Models;

public class WelcomeViewModel
{
	public event Action NavigateHelp;

	public event Action<bool, bool> NavigateStart;

	public void ProcessNavigateHelp()
	{
		NavigateHelp?.Invoke();
	}

	public void ProcessNavigateStart(bool isSpanish = false, bool isAda = false)
	{
		NavigateStart?.Invoke(isSpanish, isAda);
	}
}
