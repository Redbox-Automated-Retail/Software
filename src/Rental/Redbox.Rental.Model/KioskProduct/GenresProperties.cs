using System.Collections.Generic;

namespace Redbox.Rental.Model.KioskProduct;

public static class GenresProperties
{
	private static Dictionary<Genres, GenresProperty> _properties;

	public static Dictionary<Genres, GenresProperty> Properties
	{
		get
		{
			if (_properties == null)
			{
				_properties = new Dictionary<Genres, GenresProperty>
				{
					{
						Genres.ActionAndAdventure,
						new GenresProperty
						{
							Description = "Action And Adventure"
						}
					},
					{
						Genres.Animation,
						new GenresProperty
						{
							Description = "Animation"
						}
					},
					{
						Genres.AwardWinners,
						new GenresProperty
						{
							Description = "Award Winners"
						}
					},
					{
						Genres.Crime,
						new GenresProperty
						{
							Description = "Crime"
						}
					},
					{
						Genres.Comedy,
						new GenresProperty
						{
							Description = "Comedy"
						}
					},
					{
						Genres.Drama,
						new GenresProperty
						{
							Description = "Drama"
						}
					},
					{
						Genres.FamilyMovies,
						new GenresProperty
						{
							Description = "Family"
						}
					},
					{
						Genres.Foreign,
						new GenresProperty
						{
							Description = "Foreign"
						}
					},
					{
						Genres.Holiday,
						new GenresProperty
						{
							Description = "Holiday"
						}
					},
					{
						Genres.Horror,
						new GenresProperty
						{
							Description = "Horror"
						}
					},
					{
						Genres.Kids,
						new GenresProperty
						{
							Description = "Kids"
						}
					},
					{
						Genres.Musical,
						new GenresProperty
						{
							Description = "Musical"
						}
					},
					{
						Genres.Romance,
						new GenresProperty
						{
							Description = "Romance"
						}
					},
					{
						Genres.SciFiAndFantasy,
						new GenresProperty
						{
							Description = "SciFi And Fantasy"
						}
					},
					{
						Genres.SpecialInterest,
						new GenresProperty
						{
							Description = "Special Interest"
						}
					},
					{
						Genres.Suspense,
						new GenresProperty
						{
							Description = "Suspense"
						}
					},
					{
						Genres.Television,
						new GenresProperty
						{
							Description = "Television"
						}
					},
					{
						Genres.War,
						new GenresProperty
						{
							Description = "War"
						}
					},
					{
						Genres.HitMovies,
						new GenresProperty
						{
							Description = "Hit Movies"
						}
					},
					{
						Genres.GAME,
						new GenresProperty
						{
							Description = "GAME"
						}
					},
					{
						Genres.Bluray,
						new GenresProperty
						{
							Description = "Bluray"
						}
					},
					{
						Genres.RedboxReplayMovies,
						new GenresProperty
						{
							Description = "redbox Replay"
						}
					},
					{
						Genres.ActionGames,
						new GenresProperty
						{
							Description = "Action Games"
						}
					},
					{
						Genres.Fighting,
						new GenresProperty
						{
							Description = "Fighting"
						}
					},
					{
						Genres.MusicAndParty,
						new GenresProperty
						{
							Description = "Music And Party"
						}
					},
					{
						Genres.Shooter,
						new GenresProperty
						{
							Description = "Shooter"
						}
					},
					{
						Genres.Sports,
						new GenresProperty
						{
							Description = "Sports"
						}
					},
					{
						Genres.Top20Movies,
						new GenresProperty
						{
							Description = "Top 20 Movies"
						}
					},
					{
						Genres.FamilyGames,
						new GenresProperty
						{
							Description = "Family"
						}
					},
					{
						Genres.RedboxReplayGames,
						new GenresProperty
						{
							Description = "redbox Replay"
						}
					},
					{
						Genres.WarAndWestern,
						new GenresProperty
						{
							Description = "War And Western"
						}
					},
					{
						Genres.MartialArts,
						new GenresProperty
						{
							Description = "Martial Arts"
						}
					},
					{
						Genres.Independent,
						new GenresProperty
						{
							Description = "Independent"
						}
					},
					{
						Genres.DocumentaryAndSpecialInterest,
						new GenresProperty
						{
							Description = "Documentary And Special Interest"
						}
					},
					{
						Genres.Adventure,
						new GenresProperty
						{
							Description = "Adventure"
						}
					},
					{
						Genres.Action,
						new GenresProperty
						{
							Description = "Action"
						}
					},
					{
						Genres.AwardNominees,
						new GenresProperty
						{
							Description = "Award Nominees"
						}
					}
				};
			}
			return _properties;
		}
	}
}
