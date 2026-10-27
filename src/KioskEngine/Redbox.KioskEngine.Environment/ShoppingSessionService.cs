using System;
using System.Collections.Generic;
using System.IO;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

public class ShoppingSessionService : IShoppingSessionService, IDisposable
{
	private const string LegacyActiveSessionId = "__active__";

	private string m_currentSessionId;

	private readonly object m_syncObject = new object();

	private readonly IDictionary<string, IShoppingSession> m_sessions = new Dictionary<string, IShoppingSession>();

	private Queue<IShoppingSession> _sessionQueue = new Queue<IShoppingSession>(5);

	public static ShoppingSessionService Instance => Singleton<ShoppingSessionService>.Instance;

	public void Reset()
	{
		LogHelper.Instance.Log("Reset Shopping Session Service.");
		AbandonAll(null);
	}

	public void Dispose()
	{
		lock (m_syncObject)
		{
			Reset();
		}
	}

	public void AbandonAll(string reason)
	{
		LogHelper.Instance.Log("Adbandon all shopping sessions.");
		lock (m_syncObject)
		{
			foreach (IShoppingSession value in m_sessions.Values)
			{
				LogHelper.Instance.Log("...Marking session '{0}' abandoned and saving.", value.Id);
				value.Abandon(reason);
				value.End();
			}
			m_sessions.Clear();
		}
	}

	public IShoppingSession GetSession(string id)
	{
		lock (m_syncObject)
		{
			if (string.IsNullOrEmpty(id))
			{
				LogHelper.Instance.Log("The ID passed to GetSession is null or empty, returning null.");
				return null;
			}
			if (m_sessions.ContainsKey(id))
			{
				return m_sessions[id];
			}
			LogHelper.Instance.Log("Shopping Session '{0}' produced a cache miss, reloading from database.", id);
			IShoppingSession shoppingSession = Load(id);
			if (shoppingSession != null)
			{
				LogHelper.Instance.Log("...Session found, placing in cache.");
				m_sessions[id] = shoppingSession;
				return shoppingSession;
			}
			LogHelper.Instance.Log("Shopping Session '{0}' was not found.", id);
			return null;
		}
	}

	public IShoppingSession GetCurrentSession()
	{
		lock (m_syncObject)
		{
			if (m_sessions.ContainsKey("__active__"))
			{
				LogHelper.Instance.Log("The Shopping Session cache contains the legacy session ID, retrieiving it.");
				return m_sessions["__active__"];
			}
			return GetSession(m_currentSessionId);
		}
	}

	public string StartNewSession(string storeNumber, string sessionId)
	{
		lock (m_syncObject)
		{
			if (m_sessions.ContainsKey("__active__"))
			{
				LogHelper.Instance.Log("The Shopping Session cache contains the Legacy ID.");
				_ = m_sessions["__active__"];
			}
			if (m_sessions.Count > 0)
			{
				m_sessions.ForEach(delegate(KeyValuePair<string, IShoppingSession> s)
				{
					if (!_sessionQueue.Contains(s.Value))
					{
						_sessionQueue.Enqueue(s.Value);
					}
				});
				while (_sessionQueue.Count > 25)
				{
					_sessionQueue.Dequeue();
				}
			}
			m_sessions.Clear();
			ShoppingSession shoppingSession = new ShoppingSession((!string.IsNullOrEmpty(sessionId)) ? new Guid(sessionId) : Guid.NewGuid(), logStartEvent: true)
			{
				StartedOn = DateTime.Now,
				StoreNumber = storeNumber
			};
			shoppingSession.ShoppingCart = new ShoppingCart(shoppingSession.Id)
			{
				Type = ShoppingCartType.Online
			};
			string text = shoppingSession.Id.ToString();
			m_sessions[text] = shoppingSession;
			LogHelper.Instance.Log("Shopping Session '{0}' added to cache and started.", text);
			return text;
		}
	}

	public ErrorList Initialize(string path)
	{
		ServiceLocator.Instance.AddService(typeof(IShoppingSessionService), Instance);
		ErrorList result = new ErrorList();
		try
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception e)
		{
			LogHelper.Instance.Log("An unhandled exception was raised in ShoppingSessionServie.Initialize when trying to delete session.data.", e);
		}
		return result;
	}

	public void SetCurrentSession(string id)
	{
		LogHelper.Instance.Log("Setting current Shopping Session to: {0}", id);
		m_currentSessionId = id;
	}

	public bool StartUserInteraction()
	{
		lock (m_syncObject)
		{
			return GetCurrentSession()?.StartUserInteraction() ?? false;
		}
	}

	private ShoppingSessionService()
	{
	}

	private IShoppingSession Load(string id)
	{
		IShoppingSession found = null;
		_sessionQueue.ForEach(delegate(IShoppingSession item)
		{
			if (found == null && item.Id.Equals(new Guid(id)))
			{
				found = item;
			}
		});
		return found;
	}
}
