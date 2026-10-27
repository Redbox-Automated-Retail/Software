using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class RedChevronUserControl : UserControl, IComponentConnector
{
	private static DependencyProperty ChevronColorProperty = DependencyProperty.Register("ChevronColor", typeof(Brush), typeof(RedChevronUserControl), new PropertyMetadata((SolidColorBrush)new BrushConverter().ConvertFrom("#D01D3E")));

	private static DependencyProperty ChevronOpacityProperty = DependencyProperty.Register("ChevronOpacity", typeof(double), typeof(RedChevronUserControl), new PropertyMetadata(1.0));

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

	public Brush ChevronColor
	{
		get
		{
			return base.Dispatcher.Invoke(() => (Brush)GetValue(ChevronColorProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ChevronColorProperty, value);
			});
		}
	}

	public double ChevronOpacity
	{
		get
		{
			return base.Dispatcher.Invoke(() => (double)GetValue(ChevronOpacityProperty));
		}
		set
		{
			base.Dispatcher.Invoke(delegate
			{
				SetValue(ChevronOpacityProperty, value);
			});
		}
	}

	public RedChevronUserControl()
	{
		InitializeComponent();
	}

	public void BeginAnimation()
	{
		DoubleAnimation doubleAnimation = new DoubleAnimation(0.0, 150.0, new Duration(TimeSpan.FromSeconds(3.0)));
		doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
		TranslateTransform translateTransform = new TranslateTransform();
		ChevronContainer.RenderTransform = translateTransform;
		translateTransform.BeginAnimation(TranslateTransform.XProperty, doubleAnimation);
	}

	public void StopAnimation()
	{
		ChevronContainer.RenderTransform = null;
	}
}
