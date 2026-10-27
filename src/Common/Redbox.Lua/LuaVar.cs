namespace Redbox.Lua;

public class LuaVar
{
	public int Index { get; private set; }

	public string Name { get; private set; }

	public object Value { get; internal set; }

	public override string ToString()
	{
		return LuaHelper.FormatLuaValue(Value);
	}

	internal LuaVar(int index, string name, object value)
	{
		Index = index;
		Name = name;
		Value = value;
	}
}
