using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class ThemeService : IThemeService
{
	private List<ITheme> _themes = new List<ITheme>();

	private ITheme _currentTheme;

	public static IThemeService Instance => Singleton<ThemeService>.Instance;

	public ITheme CurrentTheme
	{
		get
		{
			return _currentTheme;
		}
		set
		{
			if (_currentTheme != value)
			{
				ITheme currentTheme = _currentTheme;
				_currentTheme = value;
				if (OnCurrentThemeChanged != null)
				{
					OnCurrentThemeChanged(currentTheme, _currentTheme);
				}
			}
		}
	}

	public ITheme DefaultTheme => _themes?.FirstOrDefault((ITheme x) => x.IsDefault);

	public List<ITheme> Themes
	{
		get
		{
			return _themes;
		}
		set
		{
			_themes = value;
		}
	}

	public event ThemeChanged OnCurrentThemeChanged;

	public void RegisterThemedControls(string assemblyName)
	{
		string text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), assemblyName);
		LogHelper.Instance.Log($"Registering themed controls in assembly {text}", LogEntryType.Info);
		if (!File.Exists(text))
		{
			return;
		}
		foreach (Type item in (from t in Assembly.LoadFile(text).GetTypes()
			where t.IsDefined(typeof(ThemedControlAttribute))
			select t).ToList())
		{
			object[] customAttributes = item.GetCustomAttributes(inherit: true);
			foreach (object obj in customAttributes)
			{
				ThemedControlAttribute themedControlAttribute = obj as ThemedControlAttribute;
				if (themedControlAttribute != null)
				{
					ITheme theme = Themes.FirstOrDefault((ITheme x) => x.Name == themedControlAttribute.ThemeName);
					if (theme == null)
					{
						theme = new Theme
						{
							Name = themedControlAttribute.ThemeName
						};
						Themes.Add(theme);
					}
					theme.AddThemedControl(themedControlAttribute.ControlName, item);
				}
			}
		}
	}

	public FrameworkElement GetNewThemedControlInstance(string controlName)
	{
		FrameworkElement frameworkElement = CurrentTheme?.GetNewThemedControlInstance(controlName);
		if (frameworkElement == null)
		{
			frameworkElement = DefaultTheme?.GetNewThemedControlInstance(controlName);
		}
		return frameworkElement;
	}

	private ThemeService()
	{
	}
}
