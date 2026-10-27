using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Redbox.Controls.Utilities;

namespace Redbox.Controls;

[Localizability(LocalizationCategory.Button)]
public class BorderButton : Button
{
	protected static List<BorderButton> borderButtons;

	public static readonly DependencyProperty ButtonStateProperty;

	public static readonly DependencyProperty IsSelectedProperty;

	public static readonly DependencyProperty CornerRadiusProperty;

	public static readonly DependencyProperty AutoCornerRadiusProperty;

	public static readonly DependencyProperty EffectColorProperty;

	public static readonly DependencyProperty SingleRadiusProperty;

	public static readonly DependencyProperty GroupNameProperty;

	public static readonly DependencyProperty BorderSizeProperty;

	public static readonly DependencyProperty BorderRectProperty;

	public static readonly DependencyProperty DisposeProperty;

	[DefaultValue("")]
	[Localizability(LocalizationCategory.NeverLocalize)]
	public string GroupName
	{
		get
		{
			return (string)GetValue(GroupNameProperty);
		}
		set
		{
			SetValue(GroupNameProperty, value);
		}
	}

	public bool IsSelected
	{
		get
		{
			return (bool)GetValue(IsSelectedProperty);
		}
		set
		{
			if (value)
			{
				UpdateSelection();
			}
			SetValue(IsSelectedProperty, value);
		}
	}

	public int ButtonState
	{
		get
		{
			return (int)GetValue(ButtonStateProperty);
		}
		set
		{
			SetValue(ButtonStateProperty, value);
		}
	}

	public bool Dispose
	{
		get
		{
			return (bool)GetValue(DisposeProperty);
		}
		set
		{
			SetValue(DisposeProperty, value);
		}
	}

	public CornerRadius CornerRadius
	{
		get
		{
			return (CornerRadius)GetValue(CornerRadiusProperty);
		}
		set
		{
			SetValue(CornerRadiusProperty, value);
		}
	}

	public double SingleRadius
	{
		get
		{
			return (double)GetValue(SingleRadiusProperty);
		}
		set
		{
			SetValue(SingleRadiusProperty, value);
		}
	}

	public bool AutoCornerRadius
	{
		get
		{
			return (bool)GetValue(AutoCornerRadiusProperty);
		}
		set
		{
			SetValue(AutoCornerRadiusProperty, value);
		}
	}

	public Size BorderSize
	{
		get
		{
			return (Size)GetValue(BorderSizeProperty);
		}
		set
		{
			SetValue(BorderSizeProperty, value);
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

	public Color EffectColor
	{
		get
		{
			return (Color)GetValue(EffectColorProperty);
		}
		set
		{
			SetValue(EffectColorProperty, value);
		}
	}

	static BorderButton()
	{
		borderButtons = new List<BorderButton>();
		ButtonStateProperty = Dependency<BorderButton>.CreateDependencyProperty("ButtonState", typeof(int), 0);
		IsSelectedProperty = Dependency<BorderButton>.CreateDependencyProperty("IsSelected", typeof(bool), false);
		CornerRadiusProperty = Dependency<BorderButton>.CreateDependencyProperty("CornerRadius", typeof(CornerRadius), new CornerRadius(0.0));
		AutoCornerRadiusProperty = Dependency<BorderButton>.CreateDependencyProperty("AutoCornerRadius", typeof(bool), false);
		EffectColorProperty = Dependency<BorderButton>.CreateDependencyProperty("EffectColor", typeof(Color), Colors.Black);
		SingleRadiusProperty = Dependency<BorderButton>.CreateDependencyProperty("SingleRadius", typeof(double), 0.0, OnSingleRadiusChanged);
		GroupNameProperty = Dependency<BorderButton>.CreateDependencyProperty("GroupName", typeof(string), string.Empty, OnGroupNameChanged);
		BorderSizeProperty = Dependency<BorderButton>.CreateDependencyProperty("BorderSize", typeof(Size), new Size(0.0, 0.0), OnBorderSizeChanged);
		BorderRectProperty = Dependency<BorderButton>.CreateDependencyProperty("BorderRect", typeof(Rect), new Rect(0.0, 0.0, 0.0, 0.0));
		DisposeProperty = Dependency<BorderButton>.CreateDependencyProperty("Dispose", typeof(bool), false, OnDisposeChanged);
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(BorderButton), new FrameworkPropertyMetadata(typeof(BorderButton)));
	}

	public BorderButton()
	{
		base.Focusable = false;
		base.ClickMode = ClickMode.Release;
	}

	protected static void Register(BorderButton borderButton)
	{
		if (!borderButtons.Contains(borderButton))
		{
			borderButtons.Add(borderButton);
		}
	}

	protected static void Unregister(BorderButton borderButton)
	{
		if (borderButtons.Contains(borderButton))
		{
			borderButtons.Remove(borderButton);
		}
	}

	private static void OnSingleRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BorderButton borderButton = d as BorderButton;
		if (!borderButton.AutoCornerRadius)
		{
			borderButton.CornerRadius = new CornerRadius(borderButton.SingleRadius);
		}
	}

	private static void OnBorderSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BorderButton obj = d as BorderButton;
		obj.Width = obj.BorderSize.Width;
		obj.Height = obj.BorderSize.Height;
	}

	private static void OnDisposeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			Unregister(d as BorderButton);
		}
	}

	private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.NewValue as string))
		{
			Unregister(d as BorderButton);
		}
		else
		{
			Register(d as BorderButton);
		}
	}

	protected void UpdateSelection()
	{
		if (string.IsNullOrEmpty(GroupName))
		{
			return;
		}
		foreach (BorderButton borderButton in borderButtons)
		{
			if (this != borderButton && GroupName.Equals(borderButton.GroupName))
			{
				borderButton.IsSelected = false;
			}
		}
	}

	protected override void OnClick()
	{
		IsSelected = true;
		base.OnClick();
	}

	protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
	{
		base.OnRenderSizeChanged(sizeInfo);
		BorderSize = new Size(sizeInfo.NewSize.Width, sizeInfo.NewSize.Height);
		BorderRect = new Rect(sizeInfo.NewSize);
		if (base.Width != BorderSize.Width)
		{
			base.Width = BorderSize.Width;
		}
		if (base.Height != BorderSize.Height)
		{
			base.Height = BorderSize.Height;
		}
		if (AutoCornerRadius)
		{
			SingleRadius = base.ActualHeight / 2.0;
			CornerRadius = new CornerRadius(SingleRadius);
		}
	}
}
