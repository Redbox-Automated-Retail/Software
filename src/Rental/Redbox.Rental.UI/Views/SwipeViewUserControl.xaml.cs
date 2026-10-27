using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TextToSpeech;
using Redbox.Rental.UI.Controls;
using Redbox.Rental.UI.Controls.Animations;

namespace Redbox.Rental.UI.Views;

[ThemedControl(ThemeName = "Redbox Classic", ControlName = "SwipeView")]
public partial class SwipeViewUserControl : TextToSpeechUserControl, IWPFActor, IComponentConnector
{
	private int m_timeout;

	private DispatcherTimer m_dispatcherTimer;

	private object m_lockObject = new object();

	private bool m_timerDisabled;

	private Dictionary<SwipeViewType, Type> _swipeViewTypeAnimationControls;

	public SwipeViewType SwipeType
	{
		get
		{
			SwipeViewType result = SwipeViewType.RegularCheckOut;
			if (AnimationContainer.Children.Count > 0)
			{
				foreach (KeyValuePair<SwipeViewType, Type> swipeViewTypeAnimationControl in _swipeViewTypeAnimationControls)
				{
					if (swipeViewTypeAnimationControl.Value == AnimationContainer.Children[0].GetType())
					{
						result = swipeViewTypeAnimationControl.Key;
					}
				}
			}
			return result;
		}
		set
		{
			AnimationContainer.Children.Clear();
			object obj = Activator.CreateInstance(_swipeViewTypeAnimationControls[value]);
			if (obj != null)
			{
				if (obj is FrameworkElement frameworkElement)
				{
					frameworkElement.VerticalAlignment = VerticalAlignment.Center;
					AnimationContainer.Children.Add(frameworkElement);
				}
				else
				{
					LogHelper.Instance.Log("Error: {0} is not a UIElement", _swipeViewTypeAnimationControls[value]);
				}
			}
			else
			{
				LogHelper.Instance.Log("Error: Unable to instanciate instance of {0}", _swipeViewTypeAnimationControls[value]);
			}
		}
	}

	public int Timeout
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
			m_dispatcherTimer.Start();
		}
	}

	private ISwipeAnimationControl SwipeAnimationControl
	{
		get
		{
			ISwipeAnimationControl result = null;
			if (AnimationContainer.Children.Count > 0)
			{
				result = AnimationContainer.Children[0] as ISwipeAnimationControl;
			}
			return result;
		}
	}

	private SwipeViewModel SwipeViewModel => base.DataContext as SwipeViewModel;

	public SwipeViewUserControl()
	{
		InitializeComponent();
		InitializeSwipeViewTypeAnimationControls();
		RegisterRoutedCommand("GoBack", SwipeViewCommands.GoBack);
	}

	public override ISpeechControl GetSpeechControl()
	{
		return SwipeViewModel?.OnGetSpeechControl();
	}

	public void StopTimer()
	{
		StopTimer(executeTimeoutEvent: false);
	}

	private void TimerTick(object sender, EventArgs e)
	{
		StopTimer(executeTimeoutEvent: true);
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
		if (flag)
		{
			SwipeViewModel?.OnTimeout?.Invoke();
		}
	}

	private void GoBackCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		StopTimer(executeTimeoutEvent: false);
		SwipeViewModel?.ProcessOnBackButtonClicked();
		HandleWPFHit();
	}

	private void InitializeSwipeViewTypeAnimationControls()
	{
		_swipeViewTypeAnimationControls = new Dictionary<SwipeViewType, Type>
		{
			{
				SwipeViewType.RegularCheckOut,
				typeof(SwipeRegularCheckOutUserControl)
			},
			{
				SwipeViewType.ReservationPickup,
				typeof(SwipeRegularCheckOutUserControl)
			},
			{
				SwipeViewType.EncryptedSwipeReservationPickup,
				typeof(EncryptedSwipeReservationPickupUserControl)
			},
			{
				SwipeViewType.EncryptedSwipeRegularCheckOut,
				typeof(EncryptedSwipeRegularCheckoutUserControl)
			},
			{
				SwipeViewType.EMVInsertTapRegularCheckout,
				typeof(EMVInsertAndTapRegularCheckoutUserControl)
			},
			{
				SwipeViewType.EMVInsertTapHideMobileRegularCheckout,
				typeof(EMVInsertAndTapHideMobileRegularCheckoutUserControl)
			},
			{
				SwipeViewType.EMVInsertTapReservationPickup,
				typeof(EMVInsertAndTapHideMobilePickupUserControl)
			},
			{
				SwipeViewType.EMVTapRegularCheckout,
				typeof(EMVTapRegularCheckoutUserControl)
			},
			{
				SwipeViewType.EMVTapReservationPickup,
				typeof(EMVTapRegularCheckoutUserControl)
			},
			{
				SwipeViewType.EMVInsertRegularCheckout,
				typeof(EMVInsertRegularCheckoutUserControl)
			},
			{
				SwipeViewType.EMVInsertReservationPickup,
				typeof(EMVInsertRegularCheckoutUserControl)
			},
			{
				SwipeViewType.EMVTapToLogin,
				typeof(EMVTapSignInUserControl)
			}
		};
	}
}
