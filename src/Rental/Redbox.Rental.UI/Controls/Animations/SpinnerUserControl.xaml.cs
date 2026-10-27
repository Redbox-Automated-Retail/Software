using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Redbox.Rental.UI.Controls.Animations;

public partial class SpinnerUserControl : UserControl, IComponentConnector
{
	private bool _isAnimated;

	public bool IsAnimated
	{
		get
		{
			return _isAnimated;
		}
		set
		{
			if (_isAnimated != value)
			{
				_isAnimated = value;
				if (_isAnimated)
				{
					SpinTheSpinner();
				}
				else
				{
					Spinner.RenderTransform = null;
				}
			}
		}
	}

	public SpinnerUserControl()
	{
		InitializeComponent();
	}

	private void SpinTheSpinner()
	{
		RotateTransform rotateTransform = new RotateTransform();
		DoubleAnimation doubleAnimation = new DoubleAnimation(0.0, 360.0, new Duration(TimeSpan.FromSeconds(1.0)));
		doubleAnimation.RepeatBehavior = RepeatBehavior.Forever;
		Spinner.RenderTransform = rotateTransform;
		rotateTransform.BeginAnimation(RotateTransform.AngleProperty, doubleAnimation);
	}
}
