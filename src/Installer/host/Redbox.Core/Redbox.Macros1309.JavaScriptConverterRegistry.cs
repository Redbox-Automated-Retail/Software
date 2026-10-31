using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace Redbox.Core;

internal class Redbox_002EMacros1309_002EJavaScriptConverterRegistry
{
	private readonly object m_syncObject = new object();

	private readonly IDictionary<string, JavaScriptConverter> m_converters = new Dictionary<string, JavaScriptConverter>();

	public static Redbox_002EMacros1309_002EJavaScriptConverterRegistry Instance => Redbox_002EMacros1309_002ESingleton<Redbox_002EMacros1309_002EJavaScriptConverterRegistry>.Instance;

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

	private Redbox_002EMacros1309_002EJavaScriptConverterRegistry()
	{
		m_converters["DateTimeConverter"] = new Redbox_002EMacros1309_002EDateTimeConverter();
		m_converters["TimeSpanConverter"] = new Redbox_002EMacros1309_002ETimeSpanConverter();
	}
}
