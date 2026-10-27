using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Redbox.Controls.Utilities;

namespace Redbox.Controls;

[Localizability(LocalizationCategory.Button)]
public class GroupButton : RoundedButton
{
	protected static List<GroupButton> groupButtons;

	public static readonly DependencyProperty ButtonStateProperty;

	public static readonly DependencyProperty DisposeProperty;

	public static readonly DependencyProperty GroupNameProperty;

	public static readonly DependencyProperty IsSelectedProperty;

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
			return (int)GetValue(GroupNameProperty);
		}
		set
		{
			SetValue(GroupNameProperty, value);
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

	static GroupButton()
	{
		groupButtons = new List<GroupButton>();
		ButtonStateProperty = Dependency<GroupButton>.CreateDependencyProperty("ButtonState", typeof(int), 0);
		DisposeProperty = Dependency<GroupButton>.CreateDependencyProperty("Dispose", typeof(bool), false, OnDisposeChanged);
		GroupNameProperty = Dependency<GroupButton>.CreateDependencyProperty("GroupName", typeof(string), string.Empty, OnGroupNameChanged);
		IsSelectedProperty = Dependency<GroupButton>.CreateDependencyProperty("IsSelected", typeof(bool), false);
		Dependency<GroupButton>.DefaultOverrideMetadata(FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata);
	}

	public GroupButton()
	{
		base.Focusable = false;
		base.ClickMode = ClickMode.Release;
	}

	protected static void Register(GroupButton groupButton)
	{
		if (!groupButtons.Contains(groupButton))
		{
			groupButtons.Add(groupButton);
		}
	}

	protected static void Unregister(GroupButton groupButton)
	{
		if (groupButtons.Contains(groupButton))
		{
			groupButtons.Remove(groupButton);
		}
	}

	private static void OnDisposeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if ((bool)e.NewValue)
		{
			Unregister(d as GroupButton);
		}
	}

	private static void OnGroupNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (string.IsNullOrEmpty(e.NewValue as string))
		{
			Unregister(d as GroupButton);
		}
		else
		{
			Register(d as GroupButton);
		}
	}

	protected void UpdateSelection()
	{
		if (string.IsNullOrEmpty(GroupName))
		{
			return;
		}
		foreach (GroupButton groupButton in groupButtons)
		{
			if (this != groupButton && GroupName.Equals(groupButton.GroupName))
			{
				groupButton.IsSelected = false;
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
	}
}
