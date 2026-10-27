using System;

namespace Redbox.HAL.Component.Model.Attributes;

public class CustomEditorAttribute : Attribute
{
	public string Text { get; private set; }

	public string GetMethodName { get; set; }

	public string SetMethodName { get; set; }

	public CustomEditorAttribute(string text)
	{
		Text = text;
	}
}
