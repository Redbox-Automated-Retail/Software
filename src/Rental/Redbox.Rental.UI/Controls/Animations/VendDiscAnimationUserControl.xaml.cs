using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class VendDiscAnimationUserControl : UserControl, IVendDiscAnimationControl, IComponentConnector
{
	private bool _isAnimated;

	public VendViewType VendType { get; set; }

	public string TitleText1
	{
		get
		{
			return TitleTextBlock1.Text;
		}
		set
		{
			TitleTextBlock1.Text = value;
		}
	}

	public string MessageText1
	{
		get
		{
			return MessageTextBlock1.Text;
		}
		set
		{
			MessageTextBlock1.Text = value;
		}
	}

	public string MessageText2
	{
		get
		{
			return MessageTextBlock2.Text;
		}
		set
		{
			MessageTextBlock2.Text = value;
		}
	}

	public string MessageText3
	{
		get
		{
			return MessageTextBlock3.Text;
		}
		set
		{
			MessageTextBlock3.Text = value;
		}
	}

	public bool IsSignedIn { get; set; }

	public bool IsPickupDisc
	{
		get
		{
			return VendDiscPickup.Visibility == Visibility.Visible;
		}
		set
		{
			if (value)
			{
				VendDiscPickup.Visibility = Visibility.Visible;
				VendDiscServe.Visibility = Visibility.Collapsed;
				TurnOffArrows();
			}
			else
			{
				VendDiscPickup.Visibility = Visibility.Collapsed;
				VendDiscServe.Visibility = Visibility.Visible;
				MessageGridBlock3.Visibility = ((!IsSignedIn) ? Visibility.Collapsed : Visibility.Visible);
				TurnOnArrows();
			}
		}
	}

	public bool IsAnimated
	{
		get
		{
			return _isAnimated;
		}
		set
		{
			if (_isAnimated != value)
			{
				_isAnimated = value;
				if (value)
				{
					BeginAnimation();
				}
				else
				{
					StopAnimation();
				}
			}
		}
	}

	public VendDiscAnimationUserControl()
	{
		InitializeComponent();
		TurnOffArrows();
	}

	public void TurnOffArrows()
	{
		MessageCheck2.Visibility = Visibility.Hidden;
		MessageCheck3.Visibility = Visibility.Hidden;
	}

	public void TurnOnArrows()
	{
		MessageCheck2.Visibility = Visibility.Visible;
		Timer timer = new Timer(1000.0);
		timer.Elapsed += delegate
		{
			Application.Current.Dispatcher.Invoke(() => MessageCheck3.Visibility = Visibility.Visible);
		};
		timer.Start();
	}

	public void BeginAnimation()
	{
		RedChevron.IsAnimated = true;
	}

	public void StopAnimation()
	{
		RedChevron.RenderTransform = null;
	}
}
