using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using Redbox.Rental.UI.Models;

namespace Redbox.Rental.UI.Controls;

public partial class DisplayProductCarouselUserControl : UserControl, IComponentConnector
{
	public static readonly DependencyProperty ShowVacantItemBackgroundProperty = DependencyProperty.Register("ShowVacantItemBackground", typeof(bool), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(false)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ShaderOpacityProperty = DependencyProperty.Register("ShaderOpacity", typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.25)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ShadowDepthProperty = DependencyProperty.Register("ShadowDepth", typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ShadowOpacityProperty = DependencyProperty.Register("ShadowOpacity", typeof(double), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(0.0)
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ImageBorderThicknessProperty = DependencyProperty.Register("ImageBorderThickness", typeof(Thickness), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(new Thickness(0.0))
	{
		AffectsRender = true
	});

	public static readonly DependencyProperty ImageBorderColorProperty = DependencyProperty.Register("ImageBorderColor", typeof(Color), typeof(DisplayProductCarouselUserControl), new FrameworkPropertyMetadata(Colors.Gray));

	private DisplayProductModel _displayProductModel;

	public bool ShowVacantItemBackground
	{
		get
		{
			return (bool)GetValue(ShowVacantItemBackgroundProperty);
		}
		set
		{
			SetValue(ShowVacantItemBackgroundProperty, value);
		}
	}

	public double ShaderOpacity
	{
		get
		{
			return (double)GetValue(ShaderOpacityProperty);
		}
		set
		{
			SetValue(ShaderOpacityProperty, value);
		}
	}

	public double ShadowDepth
	{
		get
		{
			return (double)GetValue(ShadowDepthProperty);
		}
		set
		{
			SetValue(ShadowDepthProperty, value);
		}
	}

	public double ShadowOpacity
	{
		get
		{
			return (double)GetValue(ShadowOpacityProperty);
		}
		set
		{
			SetValue(ShadowOpacityProperty, value);
		}
	}

	public Thickness ImageBorderThickness
	{
		get
		{
			return (Thickness)GetValue(ImageBorderThicknessProperty);
		}
		set
		{
			SetValue(ImageBorderThicknessProperty, value);
		}
	}

	public Color ImageBorderColor
	{
		get
		{
			return (Color)GetValue(ImageBorderColorProperty);
		}
		set
		{
			SetValue(ImageBorderColorProperty, value);
		}
	}

	public DisplayProductCarouselUserControl()
	{
		InitializeComponent();
	}

	private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		_displayProductModel = base.DataContext as DisplayProductModel;
		MainGrid.Visibility = ((_displayProductModel == null && !ShowVacantItemBackground) ? Visibility.Collapsed : Visibility.Visible);
		if (_displayProductModel == null && ShowVacantItemBackground)
		{
			MainGrid.Background = new SolidColorBrush(Colors.White);
		}
	}
}
