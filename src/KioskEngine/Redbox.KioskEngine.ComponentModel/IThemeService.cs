using System.Collections.Generic;
using System.Windows;

namespace Redbox.KioskEngine.ComponentModel;

public interface IThemeService
{
	List<ITheme> Themes { get; set; }

	ITheme CurrentTheme { get; set; }

	ITheme DefaultTheme { get; }

	event ThemeChanged OnCurrentThemeChanged;

	void RegisterThemedControls(string assemblyName);

	FrameworkElement GetNewThemedControlInstance(string controlName);
}
