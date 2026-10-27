using System.Reflection;

namespace Redbox.Core;

public class MethodInvocation
{
	private readonly object m_target;

	private readonly MethodInfo m_method;

	private readonly object[] m_parameters;

	public MethodInvocation(object target, MethodInfo method, params object[] parms)
	{
		m_target = target;
		m_method = method;
		m_parameters = parms;
	}

	public object Invoke()
	{
		if (!(m_method == null))
		{
			return m_method.Invoke(m_target, m_parameters);
		}
		return null;
	}
}
