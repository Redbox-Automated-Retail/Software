using System;
using System.Collections;

namespace TaskScheduler;

public class TaskList : IEnumerable, IDisposable
{
	private class Enumerator : IEnumerator
	{
		private ScheduledTasks outer;

		private string[] nameTask;

		private int curIndex;

		private Task curTask;

		public object Current => curTask;

		internal Enumerator(ScheduledTasks st)
		{
			outer = st;
			nameTask = st.GetTaskNames();
			Reset();
		}

		public bool MoveNext()
		{
			bool flag = ++curIndex < nameTask.Length;
			if (flag)
			{
				curTask = outer.OpenTask(nameTask[curIndex]);
			}
			return flag;
		}

		public void Reset()
		{
			curIndex = -1;
			curTask = null;
		}
	}

	private ScheduledTasks st;

	private string nameComputer;

	internal string TargetComputer
	{
		get
		{
			return nameComputer;
		}
		set
		{
			st.Dispose();
			st = new ScheduledTasks(value);
			nameComputer = value;
		}
	}

	public Task this[string name] => st.OpenTask(name);

	internal TaskList()
	{
		st = new ScheduledTasks();
	}

	internal TaskList(string computer)
	{
		st = new ScheduledTasks(computer);
	}

	public Task NewTask(string name)
	{
		return st.CreateTask(name);
	}

	public void Delete(string name)
	{
		st.DeleteTask(name);
	}

	public IEnumerator GetEnumerator()
	{
		return new Enumerator(st);
	}

	public void Dispose()
	{
		st.Dispose();
	}
}
