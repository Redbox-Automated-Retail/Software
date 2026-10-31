namespace Redbox.NAnt.TimeKeeper.Tasks;

public abstract class Primitive
{
	public override bool Equals(object obj)
	{
		if ((object)obj.GetType() == GetType())
		{
			return obj.ToString() == ToString();
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public virtual bool IsNull()
	{
		return false;
	}
}
