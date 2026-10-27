using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class EmvInsertAndTapUserControl : UserControl, IComponentConnector
{
	public enum CardReadAnimations
	{
		ChipAndContactless,
		ChipOnly,
		ContactlessOnly
	}

	private class DurationConstants
	{
		public const string InitialDelay = "InitialDelay";

		public const string FirstMotion = "FirstMotion";

		public const string PauseDelay = "PauseDelay";

		public const string FadeDelay = "FadeDelay";

		public const string EndDelay = "EndDelay";
	}

	private class KeyTimeConstants
	{
		public const string StartTime = "StartTime";

		public const string EndOfInitialDelay = "EndOfInitialDelay";

		public const string EndOfFirstMotion = "EndOfFirstMotion";

		public const string EndOfPauseDelay = "EndOfPauseDelay";

		public const string EndOfFade = "EndOfFade";

		public const string EndOfAnimation = "EndOfAnimation";
	}

	private class AnimationInfo
	{
		private Dictionary<string, TimeSpan> _durations = new Dictionary<string, TimeSpan>();

		private Dictionary<string, TimeSpan> _keyTimes = new Dictionary<string, TimeSpan>();

		public bool IsVisable { get; set; }

		public Dictionary<string, TimeSpan> Durations => _durations;

		public Dictionary<string, TimeSpan> KeyTimes => _keyTimes;

		public TimeSpan TotalDuration
		{
			get
			{
				TimeSpan zero = TimeSpan.Zero;
				if (_durations.Count > 0)
				{
					foreach (KeyValuePair<string, TimeSpan> duration in _durations)
					{
						zero += duration.Value;
					}
				}
				return zero;
			}
		}
	}

	private AnimationInfo _firstAnimationInfo;

	private AnimationInfo _secondAnimationInfo;

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

	public CardReadAnimations ShowAnimations { get; set; }

	public bool OnlyShowChipAnimation { get; set; }

	private AnimationInfo FirstAnimationInfo
	{
		get
		{
			return _firstAnimationInfo;
		}
		set
		{
			_firstAnimationInfo = value;
			if (_firstAnimationInfo != null)
			{
				_firstAnimationInfo.IsVisable = true;
			}
		}
	}

	private AnimationInfo SecondAnimationInfo
	{
		get
		{
			return _secondAnimationInfo;
		}
		set
		{
			_secondAnimationInfo = value;
			if (_secondAnimationInfo != null)
			{
				_secondAnimationInfo.IsVisable = true;
			}
		}
	}

	public EmvInsertAndTapUserControl()
	{
		InitializeComponent();
	}

	public void BeginAnimation()
	{
		Storyboard storyboard = new Storyboard();
		storyboard.RepeatBehavior = RepeatBehavior.Forever;
		TranslateTransform value = new TranslateTransform();
		RotateTransform value2 = new RotateTransform();
		TransformGroup transformGroup = new TransformGroup();
		transformGroup.Children.Add(value);
		transformGroup.Children.Add(value2);
		CardAndHand.RenderTransform = transformGroup;
		AnimationInfo animationInfo = CreatePhoneAndHandAnimationInfo();
		AnimationInfo animationInfo2 = CreateInsertChipAnimationInfo();
		FirstAnimationInfo = null;
		SecondAnimationInfo = null;
		switch (ShowAnimations)
		{
		case CardReadAnimations.ChipAndContactless:
			FirstAnimationInfo = animationInfo2;
			SecondAnimationInfo = animationInfo;
			break;
		case CardReadAnimations.ChipOnly:
			FirstAnimationInfo = animationInfo2;
			break;
		case CardReadAnimations.ContactlessOnly:
			FirstAnimationInfo = animationInfo;
			animationInfo.IsVisable = true;
			break;
		}
		if (FirstAnimationInfo != null && SecondAnimationInfo != null)
		{
			List<string> list = new List<string>();
			foreach (KeyValuePair<string, TimeSpan> keyTime in SecondAnimationInfo.KeyTimes)
			{
				list.Add(keyTime.Key);
			}
			list.ForEach(delegate(string key)
			{
				SecondAnimationInfo.KeyTimes[key] += FirstAnimationInfo.TotalDuration;
			});
		}
		CreatePhoneAndHandAnimation(storyboard, animationInfo);
		CreateInsertChipAnimation(storyboard, animationInfo2);
		storyboard.Duration = (FirstAnimationInfo?.TotalDuration ?? TimeSpan.Zero) + (SecondAnimationInfo?.TotalDuration ?? TimeSpan.Zero);
		storyboard.Begin();
	}

	private void CreateInsertChipAnimation(Storyboard storyboard, AnimationInfo insertChipAnimationInfo)
	{
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["StartTime"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = -48.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["StartTime"]),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = -110.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		if (insertChipAnimationInfo.IsVisable)
		{
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan((insertChipAnimationInfo.KeyTimes["StartTime"] > TimeSpan.Zero) ? (insertChipAnimationInfo.KeyTimes["StartTime"] - TimeSpan.FromMilliseconds(10.0)) : TimeSpan.Zero),
				Value = 0.0
			});
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["StartTime"]),
				Value = 1.0
			});
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfPauseDelay"]),
				Value = 1.0
			});
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfFade"]),
				Value = 0.0
			});
		}
		else
		{
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
				Value = 0.0
			});
		}
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, CardAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(Opacity)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames3);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames, CardAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, CardAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames2);
	}

	private static AnimationInfo CreateInsertChipAnimationInfo()
	{
		AnimationInfo animationInfo = new AnimationInfo();
		animationInfo.Durations["InitialDelay"] = TimeSpan.FromSeconds(0.75);
		animationInfo.Durations["FirstMotion"] = TimeSpan.FromSeconds(0.5);
		animationInfo.Durations["PauseDelay"] = TimeSpan.FromSeconds(2.0);
		animationInfo.Durations["FadeDelay"] = TimeSpan.FromSeconds(0.25);
		animationInfo.Durations["EndDelay"] = TimeSpan.FromSeconds(0.25);
		animationInfo.KeyTimes["StartTime"] = TimeSpan.Zero;
		animationInfo.KeyTimes["EndOfInitialDelay"] = animationInfo.KeyTimes["StartTime"] + animationInfo.Durations["InitialDelay"];
		animationInfo.KeyTimes["EndOfFirstMotion"] = animationInfo.KeyTimes["EndOfInitialDelay"] + animationInfo.Durations["FirstMotion"];
		animationInfo.KeyTimes["EndOfPauseDelay"] = animationInfo.KeyTimes["EndOfFirstMotion"] + animationInfo.Durations["PauseDelay"];
		animationInfo.KeyTimes["EndOfFade"] = animationInfo.KeyTimes["EndOfPauseDelay"] + animationInfo.Durations["FadeDelay"];
		animationInfo.KeyTimes["EndOfAnimation"] = animationInfo.KeyTimes["EndOfFade"] + animationInfo.Durations["EndDelay"];
		return animationInfo;
	}

	private AnimationInfo CreatePhoneAndHandAnimationInfo()
	{
		AnimationInfo animationInfo = new AnimationInfo();
		animationInfo.Durations["InitialDelay"] = TimeSpan.FromSeconds(0.75);
		animationInfo.Durations["FirstMotion"] = TimeSpan.FromSeconds(0.5);
		animationInfo.Durations["PauseDelay"] = TimeSpan.FromSeconds(2.0);
		animationInfo.Durations["FadeDelay"] = TimeSpan.FromSeconds(0.25);
		animationInfo.Durations["EndDelay"] = TimeSpan.FromSeconds(0.25);
		animationInfo.KeyTimes["StartTime"] = TimeSpan.Zero;
		animationInfo.KeyTimes["EndOfInitialDelay"] = animationInfo.KeyTimes["StartTime"] + animationInfo.Durations["InitialDelay"];
		animationInfo.KeyTimes["EndOfFirstMotion"] = animationInfo.KeyTimes["EndOfInitialDelay"] + animationInfo.Durations["FirstMotion"];
		animationInfo.KeyTimes["EndOfPauseDelay"] = animationInfo.KeyTimes["EndOfFirstMotion"] + animationInfo.Durations["PauseDelay"];
		animationInfo.KeyTimes["EndOfFade"] = animationInfo.KeyTimes["EndOfPauseDelay"] + animationInfo.Durations["FadeDelay"];
		animationInfo.KeyTimes["EndOfAnimation"] = animationInfo.KeyTimes["EndOfFade"] + animationInfo.Durations["EndDelay"];
		return animationInfo;
	}

	private void CreatePhoneAndHandAnimation(Storyboard storyboard, AnimationInfo phoneAndHandAnimationInfo)
	{
		TranslateTransform value = new TranslateTransform();
		RotateTransform value2 = new RotateTransform();
		TransformGroup transformGroup = new TransformGroup();
		transformGroup.Children.Add(value);
		transformGroup.Children.Add(value2);
		PhoneAndHand.RenderTransform = transformGroup;
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["StartTime"]),
			Value = 80.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 80.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = -205.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["StartTime"]),
			Value = 100.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 100.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = 28.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		if (phoneAndHandAnimationInfo.IsVisable)
		{
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan((phoneAndHandAnimationInfo.KeyTimes["StartTime"] > TimeSpan.Zero) ? (phoneAndHandAnimationInfo.KeyTimes["StartTime"] - TimeSpan.FromMilliseconds(10.0)) : TimeSpan.Zero),
				Value = 0.0
			});
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["StartTime"]),
				Value = 1.0
			});
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfPauseDelay"]),
				Value = 1.0
			});
			doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
			{
				KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfFade"]),
				Value = 0.0
			});
		}
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames4 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["StartTime"]),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 30.0
		});
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(phoneAndHandAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = 8.0
		});
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, PhoneAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(Opacity)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames3);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames, PhoneAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, PhoneAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames2);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, PhoneAndHand);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames4);
	}

	public void StopAnimation()
	{
		CardAndHand.RenderTransform = null;
	}
}
