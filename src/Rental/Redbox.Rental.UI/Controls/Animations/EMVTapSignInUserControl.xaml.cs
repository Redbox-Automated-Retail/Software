using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using Redbox.Rental.UI.Views;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class EMVTapSignInUserControl : UserControl, ISwipeAnimationControl, IComponentConnector
{
	private bool m_isAnimated;

	public bool IsAnimated
	{
		get
		{
			return m_isAnimated;
		}
		set
		{
			if (m_isAnimated != value)
			{
				m_isAnimated = value;
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

	private SwipeViewModel SwipeViewModel => base.DataContext as SwipeViewModel;

	public EMVTapSignInUserControl()
	{
		InitializeComponent();
		Phone.ShowAnimations = EMVTapPhoneUserControl.CardReadAnimations.TapPhone;
		IsAnimated = true;
	}

	public void BeginAnimation()
	{
		Phone.IsAnimated = true;
	}

	public void StopAnimation()
	{
		Phone.RenderTransform = null;
	}

	private void MessageCommand_Executed(object sender, ExecutedRoutedEventArgs e)
	{
		SwipeViewModel?.ProcessOnMessageButtonClicked();
	}
}
