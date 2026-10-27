using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class EMVTapRegularCheckoutUserControl : UserControl, ISwipeAnimationControl, IComponentConnector
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

	public EMVTapRegularCheckoutUserControl()
	{
		InitializeComponent();
		Hand.ShowAnimations = EmvInsertAndTapUserControl.CardReadAnimations.ContactlessOnly;
		IsAnimated = true;
	}

	public void BeginAnimation()
	{
		Hand.IsAnimated = true;
	}

	public void StopAnimation()
	{
		Hand.RenderTransform = null;
	}
}
