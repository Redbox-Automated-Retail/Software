using System;
using System.Windows;

namespace Redbox.KioskEngine.ComponentModel;

public interface ITheme
{
	string Name { get; set; }

	bool IsDefault { get; set; }

	bool IsAnimated { get; set; }

	FrameworkElement GetNewThemedControlInstance(string controlName);

	Type GetThemedControlType(string controlName);

	void AddThemedControl(string controlName, Type themedControlType);

	void RegisterStyleConversion(string originalStyleName, string newStyleName);

	void SetStyle(FrameworkElement frameworkElement);
}
