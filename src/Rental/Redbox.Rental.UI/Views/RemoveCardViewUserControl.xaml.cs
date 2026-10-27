using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "RemoveCardView")]
public partial class RemoveCardViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private DispatcherTimer _checkCardRemovedTimer;

	private DispatcherTimer _timeOutTimer;

	private int _checkCardRemovedTimerInterval = 1000;

	private const int _checkCardRemovedTimerIntervalMax = 5000;

	private RemoveCardViewModel RemoveCardViewModel => base.DataContext as RemoveCardViewModel;

	public RemoveCardViewUserControl()
	{
		InitializeComponent();
		_checkCardRemovedTimer = new DispatcherTimer();
		SetCheckCardRemovedTimerInterval();
		_checkCardRemovedTimer.Tick += CheckCardRemovedTimerTick;
		StartCheckCardRemovedTimer();
	}

	public override ISpeechControl GetSpeechControl()
	{
		return RemoveCardViewModel?.OnGetSpeechControl();
	}

	private void SetCheckCardRemovedTimerInterval()
	{
		if (_checkCardRemovedTimer != null)
		{
			_checkCardRemovedTimer.Interval = TimeSpan.FromMilliseconds(_checkCardRemovedTimerInterval);
		}
	}

	private void StartCheckCardRemovedTimer()
	{
		if (_checkCardRemovedTimer != null)
		{
			_checkCardRemovedTimer.Start();
		}
	}

	private void StopCheckCardRemovedTimer()
	{
		if (_checkCardRemovedTimer != null)
		{
			_checkCardRemovedTimer.Stop();
		}
	}

	private void CheckCardRemovedTimerTick(object sender, EventArgs e)
	{
		StopCheckCardRemovedTimer();
		RemoveCardViewModel?.StartCardRemovedYetCheck(RemoveCardViewModel);
	}

	private void ProcessGetCardRemovedCheckResult(bool cardWasRemoved)
	{
		if (!cardWasRemoved)
		{
			_checkCardRemovedTimerInterval = Math.Min(_checkCardRemovedTimerInterval + 1000, 5000);
			SetCheckCardRemovedTimerInterval();
			StartCheckCardRemovedTimer();
		}
	}

	private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (e.NewValue is RemoveCardViewModel removeCardViewModel)
		{
			removeCardViewModel.ProcessCardRemovedCheckResult = ProcessGetCardRemovedCheckResult;
			removeCardViewModel.StopTimeOutTimerAction = StopTimeOutTimer;
			if (removeCardViewModel.Timeout > 0 && _timeOutTimer == null)
			{
				_timeOutTimer = new DispatcherTimer();
				_timeOutTimer.Interval = TimeSpan.FromMilliseconds(removeCardViewModel.Timeout);
				_timeOutTimer.Tick += _timeOutTimer_Tick;
				_timeOutTimer.Start();
			}
		}
	}

	private void _timeOutTimer_Tick(object sender, EventArgs e)
	{
		if (_timeOutTimer != null)
		{
			StopCheckCardRemovedTimer();
			StopTimeOutTimer();
			RemoveCardViewModel?.CardNeverRemoved();
		}
	}

	private void StopTimeOutTimer()
	{
		if (_timeOutTimer != null)
		{
			_timeOutTimer.Stop();
		}
	}
}
