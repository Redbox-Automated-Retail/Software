using System.IO;
using System.Reflection;
using Redbox.Core;
using Redbox.UpdateManager.ComponentModel;

namespace Redbox.UpdateManager.Environment;

public class QueueService : IQueueService
{
	private class QueueEntry
	{
		public string Label { get; set; }

		public string Entry { get; set; }
	}

	private string m_root;

	private PersistentList<QueueEntry> m_queueFile;

	private readonly object m_queueLock = new object();

	public static QueueService Instance => Singleton<QueueService>.Instance;

	public void Initialize(string path)
	{
		string text = ServiceLocator.Instance.GetService<IMacroService>().ExpandProperties(path);
		if (!Path.IsPathRooted(text))
		{
			text = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), text);
		}
		m_root = Path.Combine(text, ".queue");
		m_queueFile = new PersistentList<QueueEntry>(m_root);
		ServiceLocator.Instance.AddService(typeof(IQueueService), this);
	}

	public string PeekRaw(string label)
	{
		foreach (QueueEntry item in m_queueFile)
		{
			if (item != null && string.Compare(item.Label, label, ignoreCase: true) == 0)
			{
				return item.Entry;
			}
		}
		return null;
	}

	public T Peek<T>(string label)
	{
		string text = PeekRaw(label);
		if (text != null)
		{
			return text.ToObject<T>();
		}
		return default(T);
	}

	public void Enqueue(string label, object entry)
	{
		QueueEntry entry2 = new QueueEntry
		{
			Label = label,
			Entry = entry.ToJson()
		};
		Write(entry2);
	}

	public void EnqueueRaw(string label, string entry)
	{
		QueueEntry entry2 = new QueueEntry
		{
			Label = label,
			Entry = entry
		};
		Write(entry2);
	}

	public void Dequeue(string label)
	{
		int num = m_queueFile.AsList().FindIndex((QueueEntry each) => each != null && string.Compare(each.Label, label, ignoreCase: true) == 0);
		if (num != -1)
		{
			m_queueFile.RemoveAt(num);
		}
	}

	private QueueService()
	{
	}

	private void Write(QueueEntry entry)
	{
		lock (m_queueLock)
		{
			m_queueFile.Add(entry);
		}
	}
}
