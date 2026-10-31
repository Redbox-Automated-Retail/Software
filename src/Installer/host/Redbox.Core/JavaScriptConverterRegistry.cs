using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Redbox.Core;

public class JavaScriptConverterRegistry
{
	private readonly object m_syncObject = new object();

	private readonly IDictionary<string, JavaScriptConverter> m_converters = new Dictionary<string, JavaScriptConverter>();

	public static JavaScriptConverterRegistry Instance => Singleton<JavaScriptConverterRegistry>.Instance;

	public void Clear()
	{
		lock (m_syncObject)
		{
			m_converters.Clear();
		}
	}

	public void AddConverter(string name, JavaScriptConverter converter)
	{
		lock (m_syncObject)
		{
			m_converters[name] = converter;
		}
	}

	public void RemoveConverter(string name)
	{
		lock (m_syncObject)
		{
			if (m_converters.ContainsKey(name))
			{
				m_converters.Remove(name);
			}
		}
	}

	public JavaScriptSerializer GetSerializer()
	{
		JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
		javaScriptSerializer.MaxJsonLength = int.MaxValue;
		JavaScriptSerializer javaScriptSerializer2 = javaScriptSerializer;
		javaScriptSerializer2.RegisterConverters(m_converters.Values.ToArray());
		return javaScriptSerializer2;
	}

	private JavaScriptConverterRegistry()
	{
		m_converters["DateTimeConverter"] = new DateTimeConverter();
		m_converters["TimeSpanConverter"] = new TimeSpanConverter();
	}
}
