using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Redbox.Controls.Themes;
using Redbox.Core;

namespace Redbox.Controls;

public class RoundedButton : Button, IThemedControl
{
	private int _executingButtonClick;

	private Storyboard _storyboard;

	public static readonly DependencyProperty StateProperty = DependencyProperty.Register("State", typeof(bool), typeof(RoundedButton), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty UsedDefaultCornerRadiusProperty = DependencyProperty.Register("UsedDefaultCornerRadius", typeof(bool), typeof(RoundedButton), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty IsClickAnimatedProperty = DependencyProperty.Register("IsClickAnimated", typeof(bool), typeof(RoundedButton), new FrameworkPropertyMetadata(true));

	public static readonly DependencyProperty IsClickBeforeAnimationProperty = DependencyProperty.Register("IsClickBeforeAnimation", typeof(bool), typeof(RoundedButton), new FrameworkPropertyMetadata(false));

	public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0, OnCornerRadiusPropertyChanged)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty CornerRadiusInnerProperty = DependencyProperty.Register("CornerRadiusInner", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty DropShadowDepthProperty = DependencyProperty.Register("DropShadowDepth", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(5.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty GlassHighlightVisibilityProperty = DependencyProperty.Register("GlassHighlightVisibility", typeof(Visibility), typeof(RoundedButton), new FrameworkPropertyMetadata(Visibility.Visible));

	public static readonly DependencyProperty GlassHighlightWidthProperty = DependencyProperty.Register("GlassHighlightWidth", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0));

	public static readonly DependencyProperty GlassHighlightHeightProperty = DependencyProperty.Register("GlassHighlightHeight", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0));

	public static readonly DependencyProperty DropShadowOpacityProperty = DependencyProperty.Register("DropShadowOpacity", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(100.0));

	public static readonly DependencyProperty GlassHighlightCornerRadiusProperty = DependencyProperty.Register("GlassHighlightCornerRadius", typeof(double), typeof(RoundedButton), new FrameworkPropertyMetadata(0.0));

	public static readonly DependencyProperty BorderColorProperty = DependencyProperty.Register("BorderColor", typeof(Color), typeof(RoundedButton), new FrameworkPropertyMetadata(Colors.Black));

	public static readonly DependencyProperty BorderGradientColorProperty = DependencyProperty.Register("BorderGradientColor", typeof(Color), typeof(RoundedButton), new FrameworkPropertyMetadata(Colors.Black));

	public static readonly DependencyProperty BorderGradientStartPointProperty = DependencyProperty.Register("BorderGradientStartPoint", typeof(Point), typeof(RoundedButton), new FrameworkPropertyMetadata(new Point(0.5, 0.0)));

	public static readonly DependencyProperty BorderGradientEndPointProperty = DependencyProperty.Register("BorderGradientEndPoint", typeof(Point), typeof(RoundedButton), new FrameworkPropertyMetadata(new Point(0.5, 1.0)));

	public static readonly DependencyProperty BackgroundColorProperty = DependencyProperty.Register("BackgroundColor", typeof(Color), typeof(RoundedButton), new FrameworkPropertyMetadata(Colors.Black));

	public static readonly DependencyProperty BackgroundGradientColorProperty = DependencyProperty.Register("BackgroundGradientColor", typeof(Color), typeof(RoundedButton), new FrameworkPropertyMetadata(Colors.Gray));

	public static readonly DependencyProperty BackgroundGradientStartPointProperty = DependencyProperty.Register("BackgroundGradientStartPoint", typeof(Point), typeof(RoundedButton), new FrameworkPropertyMetadata(new Point(0.5, 0.0)));

	public static readonly DependencyProperty BackgroundGradientEndPointProperty = DependencyProperty.Register("BackgroundGradientEndPoint", typeof(Point), typeof(RoundedButton), new FrameworkPropertyMetadata(new Point(0.5, 1.0)));

	public static bool DefaultIsAnimated { get; set; }

	public static TaskScheduler UITaskScheduler { get; set; }

	public bool State
	{
		get
		{
			return (bool)GetValue(StateProperty);
		}
		set
		{
			SetValue(StateProperty, value);
		}
	}

	public double CornerRadiusInner
	{
		get
		{
			return (double)GetValue(CornerRadiusInnerProperty);
		}
		set
		{
			SetValue(CornerRadiusInnerProperty, value);
		}
	}

	public double CornerRadius
	{
		get
		{
			return (double)GetValue(CornerRadiusProperty);
		}
		set
		{
			SetValue(CornerRadiusProperty, value);
		}
	}

	public double GlassHighlightWidth
	{
		get
		{
			return (double)GetValue(GlassHighlightWidthProperty);
		}
		set
		{
			SetValue(GlassHighlightWidthProperty, value);
		}
	}

	public double GlassHighlightHeight
	{
		get
		{
			return (double)GetValue(GlassHighlightHeightProperty);
		}
		set
		{
			SetValue(GlassHighlightHeightProperty, value);
		}
	}

	public double GlassHighlightCornerRadius
	{
		get
		{
			return (double)GetValue(GlassHighlightCornerRadiusProperty);
		}
		set
		{
			SetValue(GlassHighlightCornerRadiusProperty, value);
		}
	}

	public double DropShadowOpacity
	{
		get
		{
			return (double)GetValue(DropShadowOpacityProperty);
		}
		set
		{
			SetValue(DropShadowOpacityProperty, value);
		}
	}

	public double DropShadowDepth
	{
		get
		{
			return (double)GetValue(DropShadowDepthProperty);
		}
		set
		{
			SetValue(DropShadowDepthProperty, value);
		}
	}

	public Color BorderColor
	{
		get
		{
			return (Color)GetValue(BorderColorProperty);
		}
		set
		{
			SetValue(BorderColorProperty, value);
		}
	}

	public Color BorderGradientColor
	{
		get
		{
			return (Color)GetValue(BorderGradientColorProperty);
		}
		set
		{
			SetValue(BorderGradientColorProperty, value);
		}
	}

	public Point BorderGradientStartPoint
	{
		get
		{
			return (Point)GetValue(BorderGradientStartPointProperty);
		}
		set
		{
			SetValue(BorderGradientStartPointProperty, value);
		}
	}

	public Point BorderGradientEndPoint
	{
		get
		{
			return (Point)GetValue(BorderGradientEndPointProperty);
		}
		set
		{
			SetValue(BorderGradientEndPointProperty, value);
		}
	}

	public Color BackgroundColor
	{
		get
		{
			return (Color)GetValue(BackgroundColorProperty);
		}
		set
		{
			SetValue(BackgroundColorProperty, value);
		}
	}

	public Color BackgroundGradientColor
	{
		get
		{
			return (Color)GetValue(BackgroundGradientColorProperty);
		}
		set
		{
			SetValue(BackgroundGradientColorProperty, value);
		}
	}

	public Point BackgroundGradientStartPoint
	{
		get
		{
			return (Point)GetValue(BackgroundGradientStartPointProperty);
		}
		set
		{
			SetValue(BackgroundGradientStartPointProperty, value);
		}
	}

	public Point BackgroundGradientEndPoint
	{
		get
		{
			return (Point)GetValue(BackgroundGradientEndPointProperty);
		}
		set
		{
			SetValue(BackgroundGradientEndPointProperty, value);
		}
	}

	public Visibility GlassHighlightVisibility
	{
		get
		{
			return (Visibility)GetValue(GlassHighlightVisibilityProperty);
		}
		set
		{
			SetValue(GlassHighlightVisibilityProperty, value);
		}
	}

	public bool IsAnimated
	{
		get
		{
			return (bool)GetValue(IsClickAnimatedProperty);
		}
		set
		{
			SetValue(IsClickAnimatedProperty, value);
		}
	}

	public bool UsedDefaultCornerRadius
	{
		get
		{
			return (bool)GetValue(UsedDefaultCornerRadiusProperty);
		}
		set
		{
			SetValue(UsedDefaultCornerRadiusProperty, value);
		}
	}

	public bool IsClickBeforeAnimation
	{
		get
		{
			return (bool)GetValue(IsClickBeforeAnimationProperty);
		}
		set
		{
			SetValue(IsClickBeforeAnimationProperty, value);
		}
	}

	public RoundedButton()
	{
		base.Focusable = false;
		IsAnimated = DefaultIsAnimated;
		if (IsAnimated)
		{
			CreateAnimationStoryboard();
		}
	}

	private void CreateAnimationStoryboard()
	{
		double num = 0.05;
		double value = num * 2.0;
		_storyboard = new Storyboard
		{
			Name = "ButtonAnimationStoryboard",
			Duration = new Duration(TimeSpan.FromSeconds(value))
		};
		base.RenderTransformOrigin = new Point(0.5, 0.5);
		ScaleTransform value2 = new ScaleTransform();
		TransformGroup transformGroup = new TransformGroup();
		transformGroup.Children.Add(value2);
		base.RenderTransform = transformGroup;
		DoubleAnimation doubleAnimation = new DoubleAnimation
		{
			From = 1.0,
			To = 0.95,
			Duration = new Duration(TimeSpan.FromSeconds(num)),
			AutoReverse = true
		};
		_storyboard.Children.Add(doubleAnimation);
		Storyboard.SetTarget(doubleAnimation, this);
		Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleX)"));
		DoubleAnimation doubleAnimation2 = new DoubleAnimation
		{
			From = 1.0,
			To = 0.95,
			Duration = new Duration(TimeSpan.FromSeconds(num)),
			AutoReverse = true
		};
		_storyboard.Children.Add(doubleAnimation2);
		Storyboard.SetTarget(doubleAnimation2, this);
		Storyboard.SetTargetProperty(doubleAnimation2, new PropertyPath("(UIElement.RenderTransform).(TransformGroup.Children)[0].(ScaleY)"));
		EventTrigger eventTrigger = new EventTrigger(UIElement.PreviewMouseDownEvent);
		eventTrigger.Actions.Add(new BeginStoryboard
		{
			Storyboard = _storyboard
		});
		base.Triggers.Add(eventTrigger);
	}

	protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
	{
		if (e.ClickCount > 1)
		{
			e.Handled = true;
		}
		base.OnPreviewMouseDown(e);
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		if (UsedDefaultCornerRadius)
		{
			CornerRadius = base.ActualHeight / 2.0;
		}
		AdjustGlassHighlight(this);
		AdjustCornRadiusInner(this);
	}

	private static void AdjustGlassHighlight(RoundedButton roundedButton)
	{
		roundedButton.GlassHighlightWidth = Math.Max(roundedButton.ActualWidth - roundedButton.CornerRadius, 0.0);
		roundedButton.GlassHighlightHeight = roundedButton.CornerRadius * 0.66;
		roundedButton.GlassHighlightCornerRadius = roundedButton.CornerRadius * 0.66;
	}

	protected override void OnClick()
	{
		if (IsClickBeforeAnimation || !IsAnimated)
		{
			ButtonClick();
			return;
		}
		if (Interlocked.CompareExchange(ref _executingButtonClick, 1, 0) == 1)
		{
			LogHelper.Instance.Log("RoundedButton click is already being executed.  Ignoring this call.");
			return;
		}
		System.Timers.Timer buttonTimer = new System.Timers.Timer
		{
			AutoReset = false,
			Interval = (_storyboard?.Duration.TimeSpan.Milliseconds ?? 0) + 10
		};
		buttonTimer.Elapsed += delegate
		{
			try
			{
				buttonTimer.Stop();
				buttonTimer.Dispose();
				ButtonClick();
			}
			finally
			{
				_executingButtonClick = 0;
			}
		};
		buttonTimer.Start();
	}

	private void ButtonClick()
	{
		Task.Factory.StartNew(delegate
		{
			State = !State;
			LogButtonClick();
			base.OnClick();
		}, CancellationToken.None, TaskCreationOptions.None, (UITaskScheduler != null) ? UITaskScheduler : TaskScheduler.FromCurrentSynchronizationContext());
	}

	private void LogButtonClick()
	{
		string text = null;
		DependencyObject parent = VisualTreeHelper.GetParent(this);
		while (parent != null && !(parent is Window) && !(parent is UserControl))
		{
			parent = VisualTreeHelper.GetParent(parent);
		}
		if (parent != null && parent is UserControl)
		{
			UserControl userControl = parent as UserControl;
			text = userControl?.Name;
			if (string.IsNullOrEmpty(text))
			{
				text = userControl?.GetType()?.ToString();
			}
		}
		else if (parent != null && parent is Window)
		{
			Window window = parent as Window;
			text = window?.Name;
			if (string.IsNullOrEmpty(text))
			{
				text = window?.GetType()?.ToString();
			}
		}
		string text2 = "";
		if (!string.IsNullOrEmpty(base.Name))
		{
			text2 = "Button Name: " + base.Name;
		}
		else
		{
			string text3 = base.Tag?.ToString();
			if (!string.IsNullOrEmpty(text3))
			{
				text2 = "Tag: " + text3.Substring(0, Math.Min(text3.Length, 50));
			}
		}
		LogHelper.Instance.Log("RoundedButton click: Parent: " + text + ", " + text2);
	}

	private static void OnCornerRadiusPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		RoundedButton obj = d as RoundedButton;
		AdjustGlassHighlight(obj);
		AdjustCornRadiusInner(obj);
		obj.InvalidateProperty(FrameworkElement.WidthProperty);
	}

	private static void AdjustCornRadiusInner(RoundedButton roundedButton)
	{
		double cornerRadius = roundedButton.CornerRadius;
		if (--cornerRadius <= 0.0)
		{
			cornerRadius = 0.0;
		}
		roundedButton.CornerRadiusInner = cornerRadius;
	}
}
