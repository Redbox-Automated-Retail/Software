using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class TitleRollupOverlayUserControl : UserControl, IComponentConnector
{
	private bool _timerDisabled;

	private object _lockObject = new object();

	private DispatcherTimer _dispatcherTimer;

	private TimeSpan _timeout;

	private TimeSpan Timeout
	{
		get
		{
			return _timeout;
		}
		set
		{
			_timeout = value;
		}
	}

	public TitleRollupOverlayUserControl()
	{
		InitializeComponent();
		ApplyStyle();
	}

	public void ApplyStyle()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(DVDButton);
		obj?.SetStyle(BlurayButton);
		obj?.SetStyle(_4kUhdButton);
		obj?.SetStyle(CancelButton);
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (base.DataContext is TitleRollupOverlayModel titleRollupOverlayModel)
		{
			Timeout = titleRollupOverlayModel.Timeout;
		}
	}

	private void StartTimer()
	{
		LogHelper.Instance.Log("Starting TitleRollupOverlay timer");
		if (!(_timeout != TimeSpan.Zero))
		{
			return;
		}
		lock (_lockObject)
		{
			if (_dispatcherTimer != null)
			{
				_dispatcherTimer.Stop();
			}
			_timerDisabled = false;
			_dispatcherTimer = new DispatcherTimer();
			_dispatcherTimer.Interval = _timeout;
			_dispatcherTimer.Tick += TimerTick;
			_dispatcherTimer.Start();
		}
	}

	private void TimerTick(object sender, EventArgs e)
	{
		StopTimer(executeTimeoutEvent: true);
	}

	public void StopTimer(bool executeTimeoutEvent)
	{
		bool flag = false;
		lock (_lockObject)
		{
			_dispatcherTimer?.Stop();
			if (!_timerDisabled)
			{
				LogHelper.Instance.Log("Stopping TitleRollupOverlay timer");
				_timerDisabled = true;
				if (executeTimeoutEvent)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			LogHelper.Instance.Log("TitleRollupOverlay timed out");
			if (base.DataContext is TitleRollupOverlayModel titleRollupOverlayModel)
			{
				titleRollupOverlayModel.Visibility = Visibility.Collapsed;
				titleRollupOverlayModel.ProcessTitleRollupOverlayTimeout(titleRollupOverlayModel.BrowseItemModel, null);
			}
		}
	}

	private void CancelTitleRollupOverlayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (base.DataContext is TitleRollupOverlayModel titleRollupOverlayModel)
		{
			titleRollupOverlayModel.Visibility = Visibility.Collapsed;
			titleRollupOverlayModel.ProcessTitleRollupOverlayCancel(titleRollupOverlayModel.BrowseItemModel, e.Parameter);
		}
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (base.Visibility == Visibility.Visible)
		{
			StartTimer();
			base.Opacity = 0.0;
			AnimateOpacityChange();
		}
		else
		{
			StopTimer(executeTimeoutEvent: false);
			base.Opacity = 1.0;
		}
		if (base.DataContext is TitleRollupOverlayModel titleRollupOverlayModel)
		{
			titleRollupOverlayModel.ProcessTitleRollupVisibilityChange(base.Visibility);
		}
	}

	private void AnimateOpacityChange()
	{
		DoubleAnimation animation = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(0.1)));
		BeginAnimation(UIElement.OpacityProperty, animation);
	}

	private void AddDVDCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (CanExecuteAddDVDCommand() && base.DataContext is TitleRollupOverlayModel titleRollupOverlayModel)
		{
			titleRollupOverlayModel.Visibility = Visibility.Collapsed;
			titleRollupOverlayModel.ProcessTitleRollupOverlayAddDVD(titleRollupOverlayModel.BrowseItemModel, e.Parameter);
			Commands.ResetIdleTimerCommand.Execute(null, this);
		}
	}

	private void AddDVDCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = CanExecuteAddDVDCommand();
	}

	private bool CanExecuteAddDVDCommand()
	{
		bool result = false;
		if (base.DataContext is TitleRollupOverlayModel { DVDButtonEnabled: not false })
		{
			result = true;
		}
		return result;
	}

	private void AddBlurayCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = CanExecuteAddBlurayCommand();
	}

	private bool CanExecuteAddBlurayCommand()
	{
		bool result = false;
		if (base.DataContext is TitleRollupOverlayModel { BlurayButtonEnabled: not false })
		{
			result = true;
		}
		return result;
	}

	private void AddBlurayCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (CanExecuteAddBlurayCommand() && base.DataContext is TitleRollupOverlayModel titleRollupOverlayModel)
		{
			titleRollupOverlayModel.Visibility = Visibility.Collapsed;
			titleRollupOverlayModel.ProcessTitleRollupOverlayAddBluray(titleRollupOverlayModel.BrowseItemModel, e.Parameter);
			Commands.ResetIdleTimerCommand.Execute(null, this);
		}
	}

	private void Add4kUhdCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
	{
		e.CanExecute = CanExecuteAdd4kUhdCommand();
	}

	private bool CanExecuteAdd4kUhdCommand()
	{
		bool result = false;
		if (base.DataContext is TitleRollupOverlayModel { _4kUhdButtonEnabled: not false })
		{
			result = true;
		}
		return result;
	}

	private void Add4kUhdCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		if (CanExecuteAdd4kUhdCommand() && base.DataContext is TitleRollupOverlayModel titleRollupOverlayModel)
		{
			titleRollupOverlayModel.Visibility = Visibility.Collapsed;
			titleRollupOverlayModel.ProcessTitleRollupOverlayAdd4kUhd(titleRollupOverlayModel.BrowseItemModel, e.Parameter);
			Commands.ResetIdleTimerCommand.Execute(null, this);
		}
	}
}
