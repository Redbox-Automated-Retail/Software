namespace Redbox.HAL.Core;

public interface ICloneable<T>
{
	T Clone(params object[] parms);
}
