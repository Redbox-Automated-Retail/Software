using System;
using System.Drawing;

namespace Redbox.Extensibility.ComponentModel;

public interface IPropertyGridItem
{
	int Id { get; }

	string Name { get; }

	string Comment { get; }

	object Tag { get; set; }

	object Value { get; set; }

	bool ReadOnly { get; set; }

	object PreviousValue { get; }

	Color BackColor { get; set; }

	string DisplayName { get; set; }

	object DefaultValue { get; set; }

	PropertyEditor Editor { get; set; }

	string[] DisplayedValues { get; set; }

	event EventHandler Activated;

	event EventHandler ValueValidate;

	event EventHandler PropertyChanged;

	event EventHandler PropertyChanging;

	void RaiseActivated();

	void RaiseValueValidate();

	void RaisePropertyChanged();

	void RaisePropertyChanging();

	IPropertyGridItem GetProperty(string name);

	IPropertyGridItem AddProperty(string name, object value, string comment);

	IPropertyGridItem AddHyperlinkProperty(string name, string comment);
}
