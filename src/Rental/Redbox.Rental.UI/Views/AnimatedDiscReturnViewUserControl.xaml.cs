using System;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "AnimatedDiscReturnViewUserControl")]
public partial class AnimatedDiscReturnViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private int _timeout;

	private DispatcherTimer _dispatcherTimer;

	private readonly object _lockObject = new object();

	private bool _timerDisabled;

	private Guid? ViewFrameId { get; set; }

	public int Timeout
	{
		get
		{
			return _timeout;
		}
		set
		{
			_timeout = value;
			_dispatcherTimer?.Stop();
			_dispatcherTimer = new DispatcherTimer
			{
				Interval = TimeSpan.FromMilliseconds(_timeout)
			};
			_dispatcherTimer.Tick += TimerTick;
			_dispatcherTimer.Start();
		}
	}

	private AnimatedDiscReturnViewModel Model => base.DataContext as AnimatedDiscReturnViewModel;

	public AnimatedDiscReturnViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		RegisterRoutedCommand("GotItButton", AnimatedDiscReturnCommands.GotItButton);
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.OnGetSpeechControl();
	}

	private void ApplyTheme()
	{
		(ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme)?.SetStyle(GotItButton);
	}

	private void TimerTick(object sender, EventArgs e)
	{
		StopTimer(executeTimeoutEvent: true);
	}

	private void StopTimer(bool executeTimeoutEvent = false)
	{
		bool flag = false;
		lock (_lockObject)
		{
			_dispatcherTimer?.Stop();
			if (!_timerDisabled)
			{
				_timerDisabled = true;
				if (executeTimeoutEvent)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			Model?.ProcessOnTimeout();
		}
	}

	private void GotItButtonCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer();
		Model?.ProcessOnGotItButtonClicked();
		HandleWPFHit();
	}
}
