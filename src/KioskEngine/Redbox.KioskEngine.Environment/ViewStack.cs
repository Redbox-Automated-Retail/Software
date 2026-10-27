using System;
using System.Collections;
using System.Collections.Generic;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal class ViewStack : IEnumerable<IViewFrameInstance>, IEnumerable
{
	private sealed class ViewStackEnumerator : IEnumerator<IViewFrameInstance>, IDisposable, IEnumerator
	{
		private readonly IEnumerator<IViewFrameInstance> m_baseEnumerator;

		public IViewFrameInstance Current => m_baseEnumerator?.Current;

		object IEnumerator.Current => Current;

		public ViewStackEnumerator(IEnumerator<IViewFrameInstance> baseEnumerator)
		{
			m_baseEnumerator = baseEnumerator;
		}

		public void Dispose()
		{
			m_baseEnumerator.Dispose();
		}

		public bool MoveNext()
		{
			return m_baseEnumerator.MoveNext();
		}

		public void Reset()
		{
			m_baseEnumerator.Reset();
		}
	}

	private readonly Stack<IViewFrameInstance> m_stack = new Stack<IViewFrameInstance>();

	public int Count => m_stack.Count;

	public IEnumerator<IViewFrameInstance> GetEnumerator()
	{
		return new ViewStackEnumerator(m_stack.GetEnumerator());
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Clear()
	{
		m_stack.Clear();
	}

	public IViewFrameInstance Pop()
	{
		if (m_stack.Count != 0)
		{
			return m_stack.Pop();
		}
		return null;
	}

	public IViewFrameInstance Peek()
	{
		if (m_stack.Count != 0)
		{
			return m_stack.Peek();
		}
		return null;
	}

	public void Push(Guid id, string viewName, object parameter = null)
	{
		IBaseViewFrame viewFrame = ViewService.Instance.GetViewFrame(viewName);
		IViewFrameInstance item = new ViewFrameInstance
		{
			Id = id,
			ViewFrame = viewFrame,
			Parameter = parameter
		};
		m_stack.Push(item);
	}

	public string PeekViewName()
	{
		if (m_stack.Count != 0)
		{
			return m_stack.Peek()?.ViewFrame?.ViewName;
		}
		return null;
	}
}
