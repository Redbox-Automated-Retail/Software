namespace Redbox.Extensibility.ComponentModel;

public interface IPropertyGridCategory
{
	int Id { get; }

	string Name { get; }

	bool IsRoot { get; }

	void Clear();

	IPropertyGridCategory GetSubCategory(string name);

	IPropertyGridCategory AddSubCategory(string name);

	IPropertyGridItem GetProperty(string name);

	IPropertyGridItem AddProperty(string name, object value, string comment);

	IPropertyGridItem AddHyperlinkProperty(string name, string comment);
}
