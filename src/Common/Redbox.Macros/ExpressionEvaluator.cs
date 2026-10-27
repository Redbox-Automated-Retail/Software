using System;
using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Redbox.Macros;

[FunctionSet("property", "NAnt")]
public class ExpressionEvaluator : ExpressionEvalBase
{
	private readonly Stack m_visiting;

	private readonly Hashtable m_state;

	private readonly PropertyDictionary m_properties;

	public ExpressionEvaluator(PropertyDictionary properties, Hashtable state, Stack visiting)
	{
		m_properties = properties;
		m_state = state;
		m_visiting = visiting;
	}

	[Function("get-value")]
	public string GetPropertyValue(string propertyName)
	{
		if (m_properties.IsDynamicProperty(propertyName))
		{
			if ((string)m_state[propertyName] == "VISITING")
			{
				throw PropertyDictionary.CreateCircularException(propertyName, m_visiting);
			}
			m_visiting.Push(propertyName);
			m_state[propertyName] = "VISITING";
			string propertyValue = m_properties.GetPropertyValue(propertyName);
			if (propertyValue == null)
			{
				throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1053"), propertyName));
			}
			Location unknownLocation = Location.UnknownLocation;
			propertyValue = m_properties.ExpandProperties(propertyValue, unknownLocation, m_state, m_visiting);
			m_visiting.Pop();
			m_state[propertyName] = "VISITED";
			return propertyValue;
		}
		string text = m_properties.GetPropertyValue(propertyName);
		if (text == null)
		{
			if (m_properties.TreatUndefinedPropertyAsError)
			{
				throw new EvaluatorException(string.Format(CultureInfo.InvariantCulture, ResourceUtils.GetString("NA1053"), propertyName));
			}
			text = string.Empty;
		}
		return text;
	}

	protected override object EvaluateProperty(string propertyName)
	{
		return GetPropertyValue(propertyName);
	}

	protected override object EvaluateFunction(MethodInfo methodInfo, object[] args)
	{
		try
		{
			if (methodInfo.IsStatic)
			{
				return methodInfo.Invoke(null, args);
			}
			if (methodInfo.DeclaringType.IsAssignableFrom(typeof(ExpressionEvaluator)))
			{
				return methodInfo.Invoke(this, args);
			}
			object obj = methodInfo.DeclaringType.GetConstructor(new Type[1] { typeof(PropertyDictionary) }).Invoke(new object[1] { m_properties });
			return methodInfo.Invoke(obj, args);
		}
		catch (TargetInvocationException ex)
		{
			if (ex.InnerException != null)
			{
				throw ex.InnerException;
			}
			throw;
		}
	}
}
