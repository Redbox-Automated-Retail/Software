using System;
using System.Collections.Generic;
using Redbox.Rental.Model.Local;

namespace Redbox.Rental.Data;

public class LocalDataInstance : ILocalDataInstance
{
	private Dictionary<string, IBadCard> _badCardList = new Dictionary<string, IBadCard>();

	private ILocalData _localData;

	public Dictionary<string, IBadCard> BadCardList => _badCardList;

	public LocalDataInstance(ILocalData localData)
	{
		_localData = localData;
	}

	public bool IsAccountOnOfflineBadCardList(string hashId)
	{
		return BadCardList.ContainsKey(hashId);
	}

	public void UpdateAccountOnOfflineBadCardList(string hashId)
	{
		if (!string.IsNullOrEmpty(hashId))
		{
			IBadCard value = null;
			if (!BadCardList.TryGetValue(hashId, out value))
			{
				value = new BadCard
				{
					HashId = hashId,
					NumberOfAttempts = 0
				};
				BadCardList[hashId] = value;
			}
			value.NumberOfAttempts = ((!value.NumberOfAttempts.HasValue) ? new int?(1) : (value.NumberOfAttempts + 1));
			value.LastAttempt = DateTime.Now;
			_localData.UpdateBadCard(value);
		}
	}

	public void RemoveAccountFromOfflineBadCardList(string hashId)
	{
		IBadCard value = null;
		if (!string.IsNullOrEmpty(hashId) && BadCardList.TryGetValue(hashId, out value))
		{
			BadCardList.Remove(hashId);
			_localData.RemoveBadCard(value);
		}
	}
}
