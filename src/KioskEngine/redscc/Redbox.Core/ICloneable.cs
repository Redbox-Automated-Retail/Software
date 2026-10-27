namespace Redbox.Core;

public interface ICloneable<T>
{
	T Clone(params object[] parms);
}
