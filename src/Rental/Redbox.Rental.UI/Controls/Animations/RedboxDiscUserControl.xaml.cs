using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class RedboxDiscUserControl : UserControl, IComponentConnector
{
	public enum ScrollDirection
	{
		Right,
		Left
	}

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

	public ScrollDirection AnimationScrollDirection { get; set; }

	public RedboxDiscUserControl()
	{
		InitializeComponent();
	}

	public void BeginAnimation()
	{
		if (AnimationScrollDirection == ScrollDirection.Left)
		{
			DoubleAnimation animation = new DoubleAnimation(600.0, 0.0, new Duration(TimeSpan.FromSeconds(4.0)));
			TranslateTransform translateTransform = (TranslateTransform)(base.RenderTransform = new TranslateTransform());
			translateTransform.BeginAnimation(TranslateTransform.XProperty, animation);
			return;
		}
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(1.5)),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.FromSeconds(5.5)),
			Value = 600.0
		});
		TranslateTransform translateTransform2 = (TranslateTransform)(base.RenderTransform = new TranslateTransform());
		translateTransform2.BeginAnimation(TranslateTransform.XProperty, doubleAnimationUsingKeyFrames);
	}

	public void StopAnimation()
	{
		base.RenderTransform = null;
	}
}
