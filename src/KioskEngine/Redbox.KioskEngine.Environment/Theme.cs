using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Redbox.Controls.Themes;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class Theme : ITheme
{
	private Dictionary<string, string> _styleConversions = new Dictionary<string, string>();

	private Dictionary<string, Type> _themedControlTypes = new Dictionary<string, Type>();

	public string Name { get; set; }

	public bool IsDefault { get; set; }

	public bool IsAnimated { get; set; } = true;

	public FrameworkElement GetNewThemedControlInstance(string controlName)
	{
		FrameworkElement result = null;
		Type themedControlType = GetThemedControlType(controlName);
		if (themedControlType != null)
		{
			object obj = Activator.CreateInstance(themedControlType);
			if (obj != null)
			{
				result = obj as FrameworkElement;
			}
		}
		return result;
	}

	public Type GetThemedControlType(string controlName)
	{
		Type value = null;
		_themedControlTypes.TryGetValue(controlName, out value);
		return value;
	}

	public void AddThemedControl(string controlName, Type themedControlType)
	{
		_themedControlTypes.Add(controlName, themedControlType);
	}

	public void RegisterStyleConversion(string originalStyleName, string newStyleName)
	{
		_styleConversions.Add(originalStyleName, newStyleName);
	}

	public void SetStyle(FrameworkElement frameworkElement)
	{
		Style style = GetStyle(frameworkElement.Style, frameworkElement);
		if (style != frameworkElement.Style)
		{
			frameworkElement.Style = style;
		}
		if (frameworkElement is IThemedControl themedControl)
		{
			themedControl.IsAnimated = IsAnimated;
		}
	}

	public Style GetStyle(Style currentStyle, FrameworkElement frameworkElement)
	{
		Style style = currentStyle;
		Control control = null;
		DependencyObject dependencyObject = VisualTreeHelper.GetParent(frameworkElement);
		while (dependencyObject != null && !(dependencyObject is Window) && !(dependencyObject is UserControl))
		{
			dependencyObject = VisualTreeHelper.GetParent(dependencyObject);
		}
		if (dependencyObject == null)
		{
			FrameworkElement frameworkElement2 = frameworkElement.Parent as FrameworkElement;
			while (frameworkElement2 != null && !(frameworkElement2 is UserControl))
			{
				frameworkElement2 = frameworkElement2.Parent as FrameworkElement;
			}
			dependencyObject = frameworkElement2;
		}
		if (dependencyObject is Control control2)
		{
			string text = FindNameFromResource(currentStyle, control2);
			if (text != null)
			{
				string value = null;
				if (_styleConversions.TryGetValue(text, out value))
				{
					style = control2.FindResource(value) as Style;
					if (style == null)
					{
						style = currentStyle;
					}
				}
			}
		}
		return style;
	}

	private static string FindNameFromResource(object resourceItem, Control control)
	{
		foreach (ResourceDictionary mergedDictionary in Application.Current.Resources.MergedDictionaries)
		{
			foreach (object key in mergedDictionary.Keys)
			{
				if (mergedDictionary[key] == resourceItem)
				{
					return key.ToString();
				}
			}
		}
		return null;
	}
}
