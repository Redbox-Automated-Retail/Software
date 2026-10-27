namespace Redbox.Macros;

public abstract class FunctionSetBase
{
	protected PropertyDictionary Properties { get; set; }

	protected FunctionSetBase(PropertyDictionary properties)
	{
		Properties = properties;
	}
}
