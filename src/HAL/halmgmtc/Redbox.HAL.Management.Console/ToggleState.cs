using System;
using System.Windows.Forms;

namespace Redbox.HAL.Management.Console;

internal class ToggleState : IDisposable
{
	private Control m_control;

	public void Dispose()
	{
		if (m_control != null)
		{
			m_control.Enabled = true;
		}
	}

	public ToggleState(object o)
	{
		m_control = o as Control;
		if (m_control != null)
		{
			m_control.Enabled = false;
		}
	}
}
