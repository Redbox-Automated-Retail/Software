using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Redbox.Rental.Model.Session;

namespace Redbox.Rental.Services.Session;

public class SessionStartTriggerActionList : List<ISessionStartTriggerAction>, ISessionStartTriggerActionList, IList<ISessionStartTriggerAction>, ICollection<ISessionStartTriggerAction>, IEnumerable<ISessionStartTriggerAction>, IEnumerable
{
	private static SessionStartTriggerActionList _instance;

	public static SessionStartTriggerActionList Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = new SessionStartTriggerActionList
				{
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.BluRayBannerClicked,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.GamesBannerClicked,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.CollectionsBannerClicked,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.TurnOffBurnInViewButtonClicked,
						StartedTimedSession = false
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.ADAModeButtonClicked,
						StartedTimedSession = false
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.LanguageButtonClicked,
						StartedTimedSession = false
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.TitleRollupOverlayCancelClicked,
						StartedTimedSession = false
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.CollectionsCarouselClicked,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.ProductCarouselClicked,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.AddProductCarouselButtonClicked,
						StartedTimedSession = false
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.AddProductToCart,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.ShowStartViewFromScreenSaver,
						StartedTimedSession = false
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.ShowBrowseView,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.ShowReturnView,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.ShowHelpView,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.SignOut,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.PickupReservation,
						StartedTimedSession = true
					},
					new SessionStartTriggerAction
					{
						Name = SessionStartTriggerActionConstants.ZeroTouch,
						StartedTimedSession = true
					}
				};
			}
			return _instance;
		}
	}

	public ISessionStartTriggerAction FindByName(string name)
	{
		return this.FirstOrDefault((ISessionStartTriggerAction x) => x.Name == name);
	}
}
