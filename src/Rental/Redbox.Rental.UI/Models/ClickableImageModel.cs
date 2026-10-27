using System;
using System.Windows;

namespace Redbox.Rental.UI.Models;

public class ClickableImageModel : BaseModel<ClickableImageModel>
{
	public static readonly DependencyProperty ImageUrlProperty = BaseModel<ClickableImageModel>.CreateDependencyProperty("ImageUrl", typeof(string));

	public static readonly DependencyProperty OpacityProperty = BaseModel<ClickableImageModel>.CreateDependencyProperty("Opacity", typeof(double), 1.0);

	public static readonly DependencyProperty ZIndexProperty = BaseModel<ClickableImageModel>.CreateDependencyProperty("ZIndex", typeof(double), 0.0);

	public static readonly DependencyProperty DisplayDurationProperty = BaseModel<ClickableImageModel>.CreateDependencyProperty("DisplayDuration", typeof(double), 5.0);

	public string ImageUrl
	{
		get
		{
			return base.Dispatcher.Invoke(() => (string)GetValue(ImageUrlProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ImageUrlProperty, value);
			});
		}
	}

	public Action ClickAction { get; set; }

	public double Opacity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(OpacityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(OpacityProperty, value);
			});
		}
	}

	public double ZIndex
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(ZIndexProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ZIndexProperty, value);
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
}
