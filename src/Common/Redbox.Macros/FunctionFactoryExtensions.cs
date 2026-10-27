using System;

namespace Redbox.Macros;

public static class FunctionFactoryExtensions
{
	public static bool IsSubclassOfFunctionSetBase(this Type type)
	{
		if (type.BaseType.FullName == "System.Object")
		{
			return false;
		}
		if (type.BaseType.FullName == "Redbox.Macros.FunctionSetBase")
		{
			return true;
		}
		return type.BaseType.IsSubclassOfFunctionSetBase();
	}

	public static bool IsTypeOfExpressionEvaluator(this Type type)
	{
		return type.FullName == "Redbox.Macros.ExpressionEvaluator";
	}
}
