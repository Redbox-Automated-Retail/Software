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
		QueuedPendingCounts result = new QueuedPendingCounts();
		IQueueService service = ServiceLocator.Instance.GetService<IQueueService>();
		if (service == null)
		{
			return result;
		}
		ITrackData trackData = Redbox.Rental.Services.Session.Session.Instance?.TrackData;
		if (trackData == null || !trackData.HasValidData())
		{
			return result;
		}
		IEncryptedTrackData encryptedTrackData = trackData as IEncryptedTrackData;
		string currentCardHashId = trackData.CardHashId;
		service.ForEach(delegate(IMessage m)
		{
			Dictionary<string, object> dictionary = Encoding.ASCII.GetString(m.Data).ToDictionary();
			if (dictionary.ContainsKey("CreditCard"))
			{
				IDictionary<string, object> dictionary2 = (IDictionary<string, object>)dictionary["CreditCard"];
				if ((encryptedTrackData == null || (dictionary2.ContainsKey("BIN") && string.Compare((string)dictionary2["BIN"], encryptedTrackData.FirstSix) == 0) || (dictionary2.ContainsKey("LastFour") && string.Compare((string)dictionary2["LastFour"], encryptedTrackData.LastFour) == 0)) && dictionary2.ContainsKey("CardId") && string.Compare((string)dictionary2["CardId"], currentCardHashId) == 0 && dictionary.ContainsKey("ShoppingCart"))
				{
					foreach (IDictionary<string, object> item in (ArrayList)((IDictionary<string, object>)dictionary["ShoppingCart"])["Groups"])
					{
						if (item.ContainsKey("GroupType") && (int)item["GroupType"] == 1)
						{
							foreach (IDictionary<string, object> item2 in (ArrayList)item["Items"])
							{
								if (!item2.ContainsKey("VendStatus") || (int)item2["VendStatus"] == 0)
								{
									if (item2.ContainsKey("MultiDisc") && !(bool)item2["MultiDisc"] && item2.ContainsKey("Pricing"))
									{
										IDictionary<string, object> dictionary5 = (IDictionary<string, object>)item2["Pricing"];
										if (dictionary5.ContainsKey("InitialNight") && (string.Compare(dictionary5["InitialNight"].ToString(), "0") == 0 || string.Compare(dictionary5["InitialNight"].ToString(), "0.0") == 0))
										{
											continue;
										}
									}
									if (item2.ContainsKey("ProductFamily") && string.Compare((string)item2["ProductFamily"], "Movies", ignoreCase: true) == 0)
									{
										result.MovieRentalCount++;
									}
									if (item2.ContainsKey("ProductFamily") && string.Compare((string)item2["ProductFamily"], "Games", ignoreCase: true) == 0)
									{
										result.GameRentalCount++;
									}
								}
							}
						}
						if (item.ContainsKey("GroupType") && (int)item["GroupType"] == 2)
						{
							foreach (IDictionary<string, object> item3 in (ArrayList)item["Items"])
							{
								if (!item3.ContainsKey("VendStatus") || (int)item3["VendStatus"] == 0)
								{
									if (item3.ContainsKey("ProductFamily") && string.Compare((string)item3["ProductFamily"], "Movies", ignoreCase: true) == 0)
									{
										result.MoviePurchaseCount++;
									}
									if (item3.ContainsKey("ProductFamily") && string.Compare((string)item3["ProductFamily"], "Games", ignoreCase: true) == 0)
									{
										result.GamePurchaseCount++;
									}
								}
							}
						}
					}
				}
			}
		}, "Reconcile");
		return result;
	}
}
