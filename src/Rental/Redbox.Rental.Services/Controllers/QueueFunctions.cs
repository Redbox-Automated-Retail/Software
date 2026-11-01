using System.Collections;
using System.Collections.Generic;
using System.Text;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.KioskEngine.ComponentModel.TrackData;
using Redbox.Rental.Services.Session;

namespace Redbox.Rental.Services.Controllers;

public class QueueFunctions
{
	internal static QueuedPendingCounts GetPendingCounts()
	{
		QueuedPendingCounts counts = new QueuedPendingCounts();
		IQueueService queueService = ServiceLocator.Instance.GetService<IQueueService>();
		if (queueService == null)
		{
			return counts;
		}
		ITrackData trackData = Redbox.Rental.Services.Session.Session.Instance?.TrackData;
		if (trackData == null || !trackData.HasValidData())
		{
			return counts;
		}
		IEncryptedTrackData encryptedTrackData = trackData as IEncryptedTrackData;
		string currentCardHashId = trackData.CardHashId;
		queueService.ForEach(delegate(IMessage m)
		{
			Dictionary<string, object> reconcileMessage = Encoding.ASCII.GetString(m.Data).ToDictionary();
			if (reconcileMessage.ContainsKey("CreditCard"))
			{
				IDictionary<string, object> creditCard = (IDictionary<string, object>)reconcileMessage["CreditCard"];
				if ((encryptedTrackData == null || (creditCard.ContainsKey("BIN") && string.Compare((string)creditCard["BIN"], encryptedTrackData.FirstSix) == 0) || (creditCard.ContainsKey("LastFour") && string.Compare((string)creditCard["LastFour"], encryptedTrackData.LastFour) == 0)) && creditCard.ContainsKey("CardId") && string.Compare((string)creditCard["CardId"], currentCardHashId) == 0 && reconcileMessage.ContainsKey("ShoppingCart"))
				{
					foreach (IDictionary<string, object> group in (ArrayList)((IDictionary<string, object>)reconcileMessage["ShoppingCart"])["Groups"])
					{
						if (group.ContainsKey("GroupType") && (int)group["GroupType"] == 1)
						{
							foreach (IDictionary<string, object> rentalItem in (ArrayList)group["Items"])
							{
								if (!rentalItem.ContainsKey("VendStatus") || (int)rentalItem["VendStatus"] == 0)
								{
									if (rentalItem.ContainsKey("MultiDisc") && !(bool)rentalItem["MultiDisc"] && rentalItem.ContainsKey("Pricing"))
									{
										IDictionary<string, object> pricing = (IDictionary<string, object>)rentalItem["Pricing"];
										if (pricing.ContainsKey("InitialNight") && (string.Compare(pricing["InitialNight"].ToString(), "0") == 0 || string.Compare(pricing["InitialNight"].ToString(), "0.0") == 0))
										{
											continue;
										}
									}
									if (rentalItem.ContainsKey("ProductFamily") && string.Compare((string)rentalItem["ProductFamily"], "Movies", ignoreCase: true) == 0)
									{
										counts.MovieRentalCount++;
									}
									if (rentalItem.ContainsKey("ProductFamily") && string.Compare((string)rentalItem["ProductFamily"], "Games", ignoreCase: true) == 0)
									{
										counts.GameRentalCount++;
									}
								}
							}
						}
						if (group.ContainsKey("GroupType") && (int)group["GroupType"] == 2)
						{
							foreach (IDictionary<string, object> purchaseItem in (ArrayList)group["Items"])
							{
								if (!purchaseItem.ContainsKey("VendStatus") || (int)purchaseItem["VendStatus"] == 0)
								{
									if (purchaseItem.ContainsKey("ProductFamily") && string.Compare((string)purchaseItem["ProductFamily"], "Movies", ignoreCase: true) == 0)
									{
										counts.MoviePurchaseCount++;
									}
									if (purchaseItem.ContainsKey("ProductFamily") && string.Compare((string)purchaseItem["ProductFamily"], "Games", ignoreCase: true) == 0)
									{
										counts.GamePurchaseCount++;
									}
								}
							}
						}
					}
				}
			}
		}, "Reconcile");
		return counts;
	}
}
