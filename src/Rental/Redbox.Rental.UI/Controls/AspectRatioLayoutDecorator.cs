using System;
using System.Windows;
using System.Windows.Controls;

namespace Redbox.Rental.UI.Controls;

public class AspectRatioLayoutDecorator : Decorator
{
	public static readonly DependencyProperty AspectRatioProperty = DependencyProperty.Register("AspectRatio", typeof(double), typeof(AspectRatioLayoutDecorator), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsMeasure), ValidateAspectRatio);

	public static readonly DependencyProperty LockAspectRatioProperty = DependencyProperty.Register("LockAspectRatio", typeof(bool), typeof(AspectRatioLayoutDecorator), new FrameworkPropertyMetadata(false)
	{
		AffectsArrange = true
	});

	public bool LockAspectRatio
	{
		get
		{
			return (bool)GetValue(LockAspectRatioProperty);
		}
		set
		{
			SetValue(LockAspectRatioProperty, value);
		}
	}

	public double AspectRatio
	{
		get
		{
			return (double)GetValue(AspectRatioProperty);
		}
		set
		{
			SetValue(AspectRatioProperty, value);
		}
	}

	private static bool ValidateAspectRatio(object value)
	{
		if (!(value is double num))
		{
			return false;
		}
		if (num > 0.0 && !double.IsInfinity(num))
		{
			return !double.IsNaN(num);
		}
		return false;
	}

	protected override Size MeasureOverride(Size constraint)
	{
		if (Child != null)
		{
			constraint = SizeToRatio(constraint, expand: false);
			Child.Measure(constraint);
			if (double.IsInfinity(constraint.Width) || double.IsInfinity(constraint.Height))
			{
				return SizeToRatio(Child.DesiredSize, expand: true);
			}
			return constraint;
		}
		return new Size(0.0, 0.0);
	}

	public Size SizeToRatio(Size size, bool expand)
	{
		double aspectRatio = AspectRatio;
		double val = size.Width / aspectRatio;
		double val2 = size.Height * aspectRatio;
		if (expand)
		{
			val2 = Math.Max(val2, size.Width);
			val = Math.Max(val, size.Height);
		}
		else
		{
			val2 = Math.Min(val2, size.Width);
			val = Math.Min(val, size.Height);
		}
		return new Size(val2, val);
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		if (Child != null)
		{
			Size size = arrangeSize;
			if (LockAspectRatio)
			{
				size = SizeToRatio(arrangeSize, expand: false);
			}
			double num = arrangeSize.Width - size.Width;
			double num2 = arrangeSize.Height - size.Height;
			double y = 0.0;
			double x = 0.0;
			if (!double.IsNaN(num) && !double.IsInfinity(num))
			{
				x = num / 2.0;
			}
			if (!double.IsNaN(num2) && !double.IsInfinity(num2))
			{
				y = num2 / 2.0;
			}
			Rect finalRect = new Rect(new Point(x, y), size);
			Child.Arrange(finalRect);
		}
		return arrangeSize;
	}
}
