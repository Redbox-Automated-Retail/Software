using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class EncryptedSwipeHandAndCardUserControl : UserControl, IComponentConnector
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

	public EncryptedSwipeHandAndCardUserControl()
	{
		InitializeComponent();
	}

	public void BeginAnimation()
	{
		Storyboard obj = new Storyboard
		{
			AutoReverse = true,
			RepeatBehavior = RepeatBehavior.Forever
		};
		TranslateTransform value = new TranslateTransform();
		RotateTransform value2 = new RotateTransform();
		TransformGroup renderTransform = new TransformGroup
		{
			Children = 
			{
				(Transform)value,
				(Transform)value2
			}
		};
		CardAndHand.RenderTransform = renderTransform;
		TimeSpan timeSpan = TimeSpan.FromSeconds(0.25);
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(0.5);
		TimeSpan timeSpan3 = timeSpan + timeSpan2;
		TimeSpan timeSpan4 = TimeSpan.FromSeconds(0.5);
		TimeSpan timeSpan5 = timeSpan3 + timeSpan4;
		TimeSpan timeSpan6 = TimeSpan.FromSeconds(0.5);
		TimeSpan timeSpan7 = timeSpan5 + timeSpan6;
		TimeSpan timeSpan8 = TimeSpan.FromSeconds(0.25);
		TimeSpan timeSpan9 = timeSpan7 + timeSpan8;
		obj.Duration = timeSpan9;
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames
		{
			KeyFrames = 
			{
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
					Value = 10.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan),
					Value = 10.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan3),
					Value = 0.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan5),
					Value = 0.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan7),
					Value = 10.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan9),
					Value = 10.0
				}
			}
		};
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames
		{
			KeyFrames = 
			{
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
					Value = 100.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan),
					Value = 100.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan3),
					Value = 0.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan5),
					Value = 30.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan7),
					Value = 160.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan9),
					Value = 160.0
				}
			}
		};
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames
		{
			KeyFrames = 
			{
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
					Value = 20.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan),
					Value = 20.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan3),
					Value = -20.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan5),
					Value = 320.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan7),
					Value = 240.0
				},
				(DoubleKeyFrame)new EasingDoubleKeyFrame
				{
					KeyTime = KeyTime.FromTimeSpan(timeSpan9),
					Value = 240.0
				}
			}
		};
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, CardAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
		obj.Children.Add(doubleAnimationUsingKeyFrames2);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, CardAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
		obj.Children.Add(doubleAnimationUsingKeyFrames3);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames, CardAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));
		obj.Children.Add(doubleAnimationUsingKeyFrames);
		obj.Begin();
	}

	public void StopAnimation()
	{
		CardAndHand.RenderTransform = null;
	}
}
