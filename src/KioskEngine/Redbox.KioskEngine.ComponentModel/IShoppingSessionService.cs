using System;

namespace Redbox.KioskEngine.ComponentModel;

public interface IShoppingSessionService : IDisposable
{
	void Reset();

	string StartNewSession(string storeNumber, string sessionId);

	void AbandonAll(string reason);

	ErrorList Initialize(string path);

	void SetCurrentSession(string id);

	IShoppingSession GetCurrentSession();

	IShoppingSession GetSession(string id);

	bool StartUserInteraction();
}
