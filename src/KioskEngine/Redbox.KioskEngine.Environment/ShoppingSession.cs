using System;
using System.Collections.Generic;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;

namespace Redbox.KioskEngine.Environment;

internal sealed class ShoppingSession : IShoppingSession
{
	private bool _startedUserInteraction;

	private readonly List<IShoppingSessionError> m_errors = new List<IShoppingSessionError>();

	private readonly List<IShoppingSessionEvent> m_events = new List<IShoppingSessionEvent>();

	public Guid Id { get; internal set; }

	public DateTime? EndedOn { get; internal set; }

	public string StoreNumber { get; internal set; }

	public int ViewsShown { get; set; }

	public DateTime StartedOn { get; internal set; }

	public IShoppingCart ShoppingCart { get; internal set; }

	public ShoppingSessionStatus Status { get; internal set; }

	internal List<IShoppingSessionEvent> Events => m_events;

	internal List<IShoppingSessionError> Errors => m_errors;

	public ShoppingSession(Guid id, bool logStartEvent)
	{
		Id = id;
		if (logStartEvent)
		{
			AddEvent(ShoppingSessionEventType.Start, "Session has started.");
		}
	}

	public void AddEvent(ShoppingSessionEventType type, string description)
	{
		if (!EndedOn.HasValue)
		{
			LogHelper.Instance.Log("Added event to Shopping Session '{0}': type={1}, description={2}", Id, type, description);
			m_events.Add(new ShoppingSessionEvent
			{
				Type = type,
				TimeStamp = DateTime.Now,
				Description = description
			});
		}
	}

	public void End()
	{
		LogHelper.Instance.Log("Ending Shopping Session.");
		if (EndedOn.HasValue)
		{
			LogHelper.Instance.Log("...Session already ended, returning.");
			return;
		}
		AddEvent(ShoppingSessionEventType.End, "Session has ended.");
		EndedOn = DateTime.Now;
		LogHelper.Instance.Log("...Session EndedOn set to: {0}", EndedOn);
	}

	public void Commit(string reason)
	{
		LogHelper.Instance.Log("Commit Shopping Session '{0}'", Id);
		if (EndedOn.HasValue)
		{
			LogHelper.Instance.Log("...Shopping Session has already ended; skipping commit.");
			return;
		}
		AddEvent(ShoppingSessionEventType.Commit, (!string.IsNullOrEmpty(reason)) ? reason : "Session comitted.");
		Status = ShoppingSessionStatus.Committed;
		LogHelper.Instance.Log("...Commit complete.");
	}

	public void Abandon(string reason)
	{
		LogHelper.Instance.Log("Abandon Shopping Session '{0}'", Id);
		if (EndedOn.HasValue)
		{
			LogHelper.Instance.Log("...Shopping Session has already ended; skipping abandon.");
			return;
		}
		AddEvent(ShoppingSessionEventType.Abandon, (!string.IsNullOrEmpty(reason)) ? reason : "Session abandon.");
		Status = ShoppingSessionStatus.Abandon;
		LogHelper.Instance.Log("...Abandon complete.");
	}

	public bool StartUserInteraction()
	{
		if (_startedUserInteraction)
		{
			return false;
		}
		_startedUserInteraction = true;
		return true;
	}
}
