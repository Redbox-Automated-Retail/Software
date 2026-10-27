using System;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Redbox.Controls.BaseControls;
using Redbox.Controls.Utilities;

namespace Redbox.Controls;

[Localizability(LocalizationCategory.Ignore)]
[ContentProperty("Child")]
public class SpinControl : BaseContainer
{
	private DoubleAnimation RotateAnimation = new DoubleAnimation
	{
		From = 0.0,
		To = 360.0
	};

	public static readonly DependencyProperty RadiusProperty;

	public static readonly DependencyProperty PeriodProperty;

	public static readonly DependencyProperty BorderRectProperty;

	public static readonly DependencyProperty IsAnimatedProperty;

	public static readonly DependencyProperty RepeatForeverProperty;

	public new double Width
	{
		get
		{
			return (double)GetValue(FrameworkElement.WidthProperty);
		}
		protected set
		{
			SetValue(FrameworkElement.WidthProperty, value);
		}
	}

	public new double Height
	{
		get
		{
			return (double)GetValue(FrameworkElement.HeightProperty);
		}
		protected set
		{
			SetValue(FrameworkElement.HeightProperty, value);
		}
	}

	public double Radius
	{
		get
		{
			return (double)GetValue(RadiusProperty);
		}
		set
		{
			SetValue(RadiusProperty, value);
		}
	}

	public Rect BorderRect
	{
		get
		{
			return (Rect)GetValue(BorderRectProperty);
		}
		protected set
		{
			SetValue(BorderRectProperty, value);
		}
	}

	public bool IsAnimated
	{
		get
		{
			return (bool)GetValue(IsAnimatedProperty);
		}
		set
		{
			SetValue(IsAnimatedProperty, value);
		}
	}

	public double Period
	{
		get
		{
			return (double)GetValue(PeriodProperty);
		}
		set
		{
			SetValue(PeriodProperty, value);
		}
	}

	public bool RepeatForever
	{
		get
		{
			return (bool)GetValue(RepeatForeverProperty);
		}
		set
		{
			SetValue(RepeatForeverProperty, value);
		}
	}

	public SpinControl()
	{
		base.Focusable = false;
		base.IsVisibleChanged += OnVisiblePropertyChanged;
	}

	static SpinControl()
	{
		RadiusProperty = Dependency<SpinControl>.CreateDependencyProperty("Radius", typeof(double), 0.0, OnRadiusChanged);
		PeriodProperty = Dependency<SpinControl>.CreateDependencyProperty("Period", typeof(double), 0.0);
		BorderRectProperty = Dependency<SpinControl>.CreateDependencyProperty("BorderRect", typeof(Rect), new Rect(0.0, 0.0, 0.0, 0.0));
		IsAnimatedProperty = Dependency<SpinControl>.CreateDependencyProperty("IsAnimated", typeof(bool), false, OnIsAnimatedChanged);
		RepeatForeverProperty = Dependency<SpinControl>.CreateDependencyProperty("RepeatForever", typeof(bool), false);
		Dependency<SpinControl>.DefaultOverrideMetadata(FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata);
		FrameworkElement.WidthProperty.OverrideMetadata(typeof(SpinControl), Dependency<SectionalBar>.CreatePropertyMetadata());
		FrameworkElement.HeightProperty.OverrideMetadata(typeof(SpinControl), Dependency<SectionalBar>.CreatePropertyMetadata());
		UIElement.RenderTransformProperty.OverrideMetadata(typeof(SpinControl), Dependency<SectionalBar>.CreatePropertyMetadata(OnRenderTransformPropertyChanged));
	}

	private static void OnRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as SpinControl).RadiusChanged();
	}

	private static void OnIsAnimatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		(d as SpinControl).IsAnimatedChanged();
	}

	private static void OnRenderTransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
	}

	private void OnVisiblePropertyChanged(object s, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue && IsAnimated)
		{
			AnimationBegin();
		}
		else
		{
			AnimationEnd();
		}
	}

	protected virtual void IsAnimatedChanged()
	{
		Point renderTransformOrigin = base.RenderTransformOrigin;
		if (renderTransformOrigin.X != 0.5 || renderTransformOrigin.Y != 0.5)
		{
			base.RenderTransformOrigin = new Point(0.5, 0.5);
		}
		if (base.RenderTransform == null || !(base.RenderTransform is RotateTransform))
		{
			base.RenderTransform = new RotateTransform();
		}
		(base.RenderTransform as RotateTransform).Angle = 0.0;
	}

	protected virtual void RadiusChanged()
	{
		double width = (Height = Radius * 2.0);
		Width = width;
	}

	protected virtual void AnimationEnd()
	{
		if (base.RenderTransform != null)
		{
			RotateAnimation.BeginTime = null;
			base.RenderTransform = null;
		}
	}

	protected virtual void AnimationBegin()
	{
		SetChildSpinSize();
		RotateAnimation.BeginTime = TimeSpan.FromMilliseconds(0.0);
		RotateAnimation.Duration = new Duration(TimeSpan.FromSeconds(Period));
		if (RepeatForever)
		{
			RotateAnimation.RepeatBehavior = RepeatBehavior.Forever;
		}
		if (base.RenderTransform == null)
		{
			IsAnimatedChanged();
		}
		(base.RenderTransform as RotateTransform)?.BeginAnimation(RotateTransform.AngleProperty, RotateAnimation);
	}

	protected void SetChildSpinSize(Size? size = null)
	{
		size = size ?? new Size?(new Size(Width, Height));
		if (Child is FrameworkElement)
		{
			FrameworkElement obj = Child as FrameworkElement;
			obj.Width = size.Value.Width;
			obj.Height = size.Value.Height;
		}
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		BorderRect = new Rect(sizeInfo.NewSize);
		SetChildSpinSize(sizeInfo.NewSize);
		base.OnRenderSizeChanged(sizeInfo);
	}
}
