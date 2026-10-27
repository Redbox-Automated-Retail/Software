using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class CrossfadeImageRotatorUserControl : UserControl, IComponentConnector, IStyleConnector
{
	private static DependencyProperty ImageModelsProperty = DependencyProperty.Register("ImageModels", typeof(ObservableCollection<ClickableImageModel>), typeof(CrossfadeImageRotatorUserControl), new PropertyMetadata(null, ModelsChanged));

	public static readonly DependencyProperty DisplayDurationProperty = DependencyProperty.Register("DisplayDuration", typeof(double), typeof(CrossfadeImageRotatorUserControl), new PropertyMetadata(5.0, DisplayDurationChanged));

	public static readonly DependencyProperty FadeDurationProperty = DependencyProperty.Register("FadeDuration", typeof(double), typeof(CrossfadeImageRotatorUserControl), new PropertyMetadata(0.5));

	public static readonly DependencyProperty IsAnimationOnProperty = DependencyProperty.Register("IsAnimationOn", typeof(bool), typeof(CrossfadeImageRotatorUserControl), new PropertyMetadata(false, IsAnimationOnChanged));

	private int _currentImageIndex;

	private int _nextImageIndex;

	private DispatcherTimer _timer;

	private bool _timerStarted;

	private bool _isRotating;

	private Storyboard _fadeStoryBoard;

	public ObservableCollection<ClickableImageModel> ImageModels
	{
		get
		{
			return base.Dispatcher.Invoke(() => (ObservableCollection<ClickableImageModel>)GetValue(ImageModelsProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageModelsProperty, value);
			});
		}
	}

	public double DisplayDuration
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(DisplayDurationProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(DisplayDurationProperty, value);
			});
		}
	}

	public double FadeDuration
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(FadeDurationProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(FadeDurationProperty, value);
			});
		}
	}

	public bool IsAnimationOn
	{
		get
		{
			return base.Dispatcher.Invoke(() => (bool)GetValue(IsAnimationOnProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(IsAnimationOnProperty, value);
			});
		}
	}

	private MarketingScreenSaverModel MarketingScreenSaverModel => base.DataContext as MarketingScreenSaverModel;

	public CrossfadeImageRotatorUserControl()
	{
		InitializeComponent();
		ConfigureTimer();
	}

	private static void ModelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as CrossfadeImageRotatorUserControl).InitializeModels();
	}

	public void InitializeModels()
	{
		if (ImageModels == null)
		{
			return;
		}
		for (int i = 0; i < ImageModels.Count; i++)
		{
			ClickableImageModel clickableImageModel = ImageModels[i];
			if (i == 0)
			{
				clickableImageModel.Opacity = 1.0;
			}
			else
			{
				clickableImageModel.Opacity = 0.0;
			}
			clickableImageModel.ZIndex = ImageModels.Count - i;
		}
		_currentImageIndex = 0;
		DisplayDuration = ((ImageModels.Count >= _currentImageIndex + 1) ? ImageModels[_currentImageIndex].DisplayDuration : 5.0);
	}

	private static void DisplayDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is CrossfadeImageRotatorUserControl crossfadeImageRotatorUserControl)
		{
			crossfadeImageRotatorUserControl.ConfigureTimer();
		}
	}

	private static void IsAnimationOnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (d is CrossfadeImageRotatorUserControl crossfadeImageRotatorUserControl)
		{
			if ((bool)e.NewValue)
			{
				crossfadeImageRotatorUserControl.StartTimer();
			}
			else
			{
				crossfadeImageRotatorUserControl.StopTimer();
			}
		}
	}

	private void Button_Click(object sender, RoutedEventArgs e)
	{
		if (!_isRotating && ImageModels != null && ImageModels.Count >= _currentImageIndex + 1 && ImageModels[_currentImageIndex].ClickAction != null)
		{
			StopTimer();
			ImageModels[_currentImageIndex].ClickAction();
			InitializeModels();
		}
	}

	private void StartTimer()
	{
		if (IsAnimationOn && !_timerStarted)
		{
			_timerStarted = true;
			_timer.Start();
		}
	}

	private void StopTimer()
	{
		_timer.Stop();
		_timerStarted = false;
	}

	private void ConfigureTimer()
	{
		_timer = new DispatcherTimer(DispatcherPriority.Send);
		_timer.Tick += _timer_Tick;
		_timer.Interval = TimeSpan.FromSeconds(DisplayDuration);
	}

	private void _timer_Tick(object sender, EventArgs e)
	{
		StopTimer();
		if (_currentImageIndex >= ImageModels.Count - 1)
		{
			MarketingScreenSaverModel?.OnLeaveLastImage?.Invoke();
			InitializeModels();
		}
		else
		{
			AnimateImageRotation();
		}
	}

	private void AnimateImageRotation()
	{
		_isRotating = true;
		TimeSpan timeSpan = TimeSpan.FromSeconds(FadeDuration);
		TimeSpan timeSpan2 = TimeSpan.FromSeconds(FadeDuration / 2.0);
		_fadeStoryBoard?.Stop();
		_fadeStoryBoard = new Storyboard();
		_fadeStoryBoard.Duration = timeSpan;
		_fadeStoryBoard.Completed += FadeStoryBoard_Completed;
		_nextImageIndex = _currentImageIndex + 1;
		if (_nextImageIndex >= ImageModels.Count)
		{
			_nextImageIndex = 0;
		}
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 1.0
		});
		doubleAnimationUsingKeyFrames.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan),
			Value = 0.0
		});
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames, ImageModels[_currentImageIndex]);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames, new PropertyPath("Opacity"));
		_fadeStoryBoard.Children.Add(doubleAnimationUsingKeyFrames);
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames2 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero),
			Value = 0.0
		});
		doubleAnimationUsingKeyFrames2.KeyFrames.Add(new EasingDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan),
			Value = 1.0
		});
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames2, ImageModels[_nextImageIndex]);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames2, new PropertyPath("Opacity"));
		_fadeStoryBoard.Children.Add(doubleAnimationUsingKeyFrames2);
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames3 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames3.KeyFrames.Add(new DiscreteDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan2),
			Value = 0.0
		});
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames3, ImageModels[_currentImageIndex]);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames3, new PropertyPath("ZIndex"));
		_fadeStoryBoard.Children.Add(doubleAnimationUsingKeyFrames3);
		DoubleAnimationUsingKeyFrames doubleAnimationUsingKeyFrames4 = new DoubleAnimationUsingKeyFrames();
		doubleAnimationUsingKeyFrames4.KeyFrames.Add(new DiscreteDoubleKeyFrame
		{
			KeyTime = KeyTime.FromTimeSpan(timeSpan2),
			Value = ImageModels.Count
		});
		Storyboard.SetTarget(doubleAnimationUsingKeyFrames4, ImageModels[_nextImageIndex]);
		Storyboard.SetTargetProperty(doubleAnimationUsingKeyFrames4, new PropertyPath("ZIndex"));
		_fadeStoryBoard.Children.Add(doubleAnimationUsingKeyFrames4);
		_fadeStoryBoard.Begin();
	}

	private void FadeStoryBoard_Completed(object sender, EventArgs e)
	{
		if (_currentImageIndex == 0 && ImageModels != null && ImageModels.Count >= 1)
		{
			ImageModels[0].Opacity = 0.0;
		}
		_currentImageIndex = _nextImageIndex;
		_isRotating = false;
		DisplayDuration = ((ImageModels != null && ImageModels.Count >= _currentImageIndex + 1) ? ImageModels[_currentImageIndex].DisplayDuration : 5.0);
		StartTimer();
	}
}
