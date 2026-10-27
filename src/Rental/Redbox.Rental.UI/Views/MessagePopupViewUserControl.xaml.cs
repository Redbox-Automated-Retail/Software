using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "MessagePopupViewUserControl")]
public partial class MessagePopupViewUserControl : TextToSpeechUserControl, IComponentConnector
{
	private int m_timeout;

	private DispatcherTimer m_dispatcherTimer;

	private object m_lockObject = new object();

	private bool m_timerDisabled;

	private Guid? ViewFrameId { get; set; }

	private int Timeout
	{
		get
		{
			return m_timeout;
		}
		set
		{
			m_timeout = value;
			if (m_dispatcherTimer != null)
			{
				m_dispatcherTimer.Stop();
			}
			m_dispatcherTimer = new DispatcherTimer();
			m_dispatcherTimer.Interval = new TimeSpan(m_timeout * 10000);
			m_dispatcherTimer.Tick += TimerTick;
			if (m_timeout > 0)
			{
				m_dispatcherTimer.Start();
			}
		}
	}

	private MessagePopupViewModel Model => base.DataContext as MessagePopupViewModel;

	public MessagePopupViewUserControl()
	{
		InitializeComponent();
		ApplyTheme();
		RegisterRoutedCommand("Button1", MessagePopupViewCommands.Button1);
		RegisterRoutedCommand("Button2", MessagePopupViewCommands.Button2);
		ViewFrameId = (ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame())?.Id;
	}

	private void ApplyTheme()
	{
		ITheme obj = ServiceLocator.Instance.GetService<IThemeService>()?.CurrentTheme;
		obj?.SetStyle(Button1);
		obj?.SetStyle(FloatingButton1);
		obj?.SetStyle(Button2);
		obj?.SetStyle(FloatingButton2);
	}

	public void StopTimer()
	{
		Model.OnTimeOut = null;
		StopTimer(executeTimeoutEvent: false);
	}

	public void ReenableTimer()
	{
		m_timerDisabled = false;
	}

	private void TimerTick(object sender, EventArgs e)
	{
		IViewFrameInstance viewFrameInstance = ServiceLocator.Instance.GetService<IViewService>()?.PeekViewFrame();
		bool flag = viewFrameInstance?.Id == ViewFrameId;
		if (!flag)
		{
			string arg = viewFrameInstance?.ViewFrame?.ViewName ?? "Unknown View";
			LogHelper.Instance.Log($"message_popup_view time out when on {arg}");
		}
		StopTimer(flag);
	}

	private void StopTimer(bool executeTimeoutEvent)
	{
		bool flag = false;
		lock (m_lockObject)
		{
			m_dispatcherTimer?.Stop();
			if (!m_timerDisabled)
			{
				m_timerDisabled = true;
				if (executeTimeoutEvent)
				{
					flag = true;
				}
			}
		}
		if (flag && Model?.OnTimeOut != null)
		{
			Model.OnTimeOut();
		}
	}

	private void Button1CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				if (!base.IsMultiClickBlocked)
				{
					base.IsMultiClickBlocked = true;
					StopTimer();
					if (Model?.OnButton1Clicked != null)
					{
						Model.OnButton1Clicked();
					}
				}
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void Button2CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		RunCommandOnce(delegate
		{
			try
			{
				if (!base.IsMultiClickBlocked)
				{
					base.IsMultiClickBlocked = true;
					StopTimer();
					if (Model?.OnButton2Clicked != null)
					{
						Model.OnButton2Clicked();
					}
				}
			}
			finally
			{
				CompleteRunOnce();
			}
		});
	}

	private void TextToSpeechUserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (Model == null)
		{
			return;
		}
		Timeout = Model.TimeOut;
		if (Model.ContentUserControl != null)
		{
			if (Model.AutoSizedGridVisibility == Visibility.Visible)
			{
				AutoSizeGrid_UserControlContainer.Children.Clear();
				AutoSizeGrid_UserControlContainer.Children.Add(Model.ContentUserControl);
			}
			else
			{
				UserControlContainer.Children.Clear();
				UserControlContainer.Children.Add(Model.ContentUserControl);
			}
		}
		if (string.IsNullOrEmpty(Model.Button1Text))
		{
			ButtonGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
			FloatingButtonGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
			AutoSizeGrid_ButtonGrid.ColumnDefinitions[0].Width = new GridLength(0.0);
		}
		if (string.IsNullOrEmpty(Model.Button2Text))
		{
			ButtonGrid.ColumnDefinitions[1].Width = new GridLength(0.0);
			FloatingButtonGrid.ColumnDefinitions[1].Width = new GridLength(0.0);
			AutoSizeGrid_ButtonGrid.ColumnDefinitions[1].Width = new GridLength(0.0);
		}
	}

	public override ISpeechControl GetSpeechControl()
	{
		return Model?.OnGetSpeechControl();
	}
}
