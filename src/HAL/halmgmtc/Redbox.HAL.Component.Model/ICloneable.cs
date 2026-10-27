namespace Redbox.HAL.Component.Model;

public interface ICloneable<T>
{
	T Clone(params object[] parms);
}
