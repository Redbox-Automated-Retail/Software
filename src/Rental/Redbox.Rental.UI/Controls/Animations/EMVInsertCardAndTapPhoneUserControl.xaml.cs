using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class EMVInsertCardAndTapPhoneUserControl : UserControl, IComponentConnector
{
	public enum CardReadAnimations
	{
		InsertChipAndTapPhone
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

	private AnimationInfo FirstAnimationInfo { get; set; }

	private AnimationInfo SecondAnimationInfo { get; set; }

	private AnimationInfo ThirdAnimationInfo { get; set; }

	public EMVInsertCardAndTapPhoneUserControl()
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
		InsertChip.RenderTransform = transformGroup;
		AnimationInfo animationInfo = CreateInsertChipAnimationInfo();
		AnimationInfo animationInfo2 = CreateTapPhoneAnimationInfo();
		AnimationInfo animationInfo3 = CreateTapPhoneCheckmarkAnimationInfo();
		FirstAnimationInfo = null;
		SecondAnimationInfo = null;
		ThirdAnimationInfo = null;
		if (ShowAnimations == CardReadAnimations.InsertChipAndTapPhone)
		{
			FirstAnimationInfo = animationInfo;
			SecondAnimationInfo = animationInfo2;
			ThirdAnimationInfo = animationInfo3;
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
		if (SecondAnimationInfo != null && ThirdAnimationInfo != null)
		{
			List<string> list2 = new List<string>();
			foreach (KeyValuePair<string, TimeSpan> keyTime2 in ThirdAnimationInfo.KeyTimes)
			{
				list2.Add(keyTime2.Key);
			}
			list2.ForEach(delegate(string key)
			{
				ThirdAnimationInfo.KeyTimes[key] += FirstAnimationInfo.TotalDuration + SecondAnimationInfo.TotalDuration;
			});
		}
		CreateInsertChipAnimation(storyboard, animationInfo);
		CreateTapPhoneAnimation(storyboard, animationInfo2);
		CreateTapPhoneCheckmarkAnimation(storyboard, animationInfo3);
		storyboard.Duration = (FirstAnimationInfo?.TotalDuration ?? TimeSpan.Zero) + (SecondAnimationInfo?.TotalDuration ?? TimeSpan.Zero) + (ThirdAnimationInfo?.TotalDuration ?? TimeSpan.Zero);
		storyboard.Begin();
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
			Value = -90.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["StartTime"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(insertChipAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = -90.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
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
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames4 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 25.0
		});
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames, InsertChip);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, InsertChip);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames2);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, InsertChip);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(Opacity)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames3);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, InsertChip);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames4);
	}

	private AnimationInfo CreateTapPhoneAnimationInfo()
	{
		AnimationInfo animationInfo = new AnimationInfo();
		animationInfo.Durations["InitialDelay"] = TimeSpan.FromSeconds(0.75);
		animationInfo.Durations["FirstMotion"] = TimeSpan.FromSeconds(0.5);
		animationInfo.Durations["PauseDelay"] = TimeSpan.FromSeconds(0.25);
		animationInfo.Durations["FadeDelay"] = TimeSpan.FromSeconds(0.0);
		animationInfo.Durations["EndDelay"] = TimeSpan.FromSeconds(0.0);
		animationInfo.KeyTimes["StartTime"] = TimeSpan.Zero;
		animationInfo.KeyTimes["EndOfInitialDelay"] = animationInfo.KeyTimes["StartTime"] + animationInfo.Durations["InitialDelay"];
		animationInfo.KeyTimes["EndOfFirstMotion"] = animationInfo.KeyTimes["EndOfInitialDelay"] + animationInfo.Durations["FirstMotion"];
		animationInfo.KeyTimes["EndOfPauseDelay"] = animationInfo.KeyTimes["EndOfFirstMotion"] + animationInfo.Durations["PauseDelay"];
		animationInfo.KeyTimes["EndOfFade"] = animationInfo.KeyTimes["EndOfPauseDelay"] + animationInfo.Durations["FadeDelay"];
		animationInfo.KeyTimes["EndOfAnimation"] = animationInfo.KeyTimes["EndOfFade"] + animationInfo.Durations["EndDelay"];
		return animationInfo;
	}

	private void CreateTapPhoneAnimation(Storyboard storyboard, AnimationInfo tapPhoneAnimationInfo)
	{
		TranslateTransform value = new TranslateTransform();
		RotateTransform value2 = new RotateTransform();
		TransformGroup transformGroup = new TransformGroup();
		transformGroup.Children.Add(value);
		transformGroup.Children.Add(value2);
		TapPhone.RenderTransform = transformGroup;
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["StartTime"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = 153.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["StartTime"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = -230.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan((tapPhoneAnimationInfo.KeyTimes["StartTime"] > TimeSpan.Zero) ? (tapPhoneAnimationInfo.KeyTimes["StartTime"] - TimeSpan.FromMilliseconds(10.0)) : TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["StartTime"]),
			Value = 1.0
		});
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["EndOfPauseDelay"]),
			Value = 1.0
		});
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["EndOfFade"]),
			Value = 0.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames4 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["StartTime"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = 18.0
		});
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames, TapPhone);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, TapPhone);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames2);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, TapPhone);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(Opacity)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames3);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, TapPhone);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[1].(RotateTransform.Angle)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames4);
	}

	private AnimationInfo CreateTapPhoneCheckmarkAnimationInfo()
	{
		AnimationInfo animationInfo = new AnimationInfo();
		animationInfo.Durations["InitialDelay"] = TimeSpan.FromSeconds(0.0);
		animationInfo.Durations["FirstMotion"] = TimeSpan.FromSeconds(0.0);
		animationInfo.Durations["PauseDelay"] = TimeSpan.FromSeconds(1.75);
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

	private void CreateTapPhoneCheckmarkAnimation(Storyboard storyboard, AnimationInfo tapPhoneCheckmarkAnimationInfo)
	{
		TranslateTransform value = new TranslateTransform();
		TransformGroup transformGroup = new TransformGroup();
		transformGroup.Children.Add(value);
		TapPhoneCheckmark.RenderTransform = transformGroup;
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 175.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneCheckmarkAnimationInfo.KeyTimes["StartTime"]),
			Value = 175.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneCheckmarkAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = 175.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneCheckmarkAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = 175.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = -150.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneCheckmarkAnimationInfo.KeyTimes["StartTime"]),
			Value = -150.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneCheckmarkAnimationInfo.KeyTimes["EndOfInitialDelay"]),
			Value = -150.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneCheckmarkAnimationInfo.KeyTimes["EndOfFirstMotion"]),
			Value = -150.0
		});
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan((tapPhoneCheckmarkAnimationInfo.KeyTimes["StartTime"] > TimeSpan.Zero) ? (tapPhoneCheckmarkAnimationInfo.KeyTimes["StartTime"] - TimeSpan.FromMilliseconds(10.0)) : TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneCheckmarkAnimationInfo.KeyTimes["StartTime"]),
			Value = 1.0
		});
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneCheckmarkAnimationInfo.KeyTimes["EndOfPauseDelay"]),
			Value = 1.0
		});
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(tapPhoneCheckmarkAnimationInfo.KeyTimes["EndOfFade"]),
			Value = 0.0
		});
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames, TapPhoneCheckmark);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.X)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, TapPhoneCheckmark);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(TranslateTransform.Y)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames2);
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, TapPhoneCheckmark);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("(Opacity)"));
		storyboard.Children.Add(doubleAnimationUsingKeyFrames3);
	}

	public void StopAnimation()
	{
		InsertChip.RenderTransform = null;
	}
}
