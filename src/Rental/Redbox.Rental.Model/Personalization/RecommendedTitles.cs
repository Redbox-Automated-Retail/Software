using System.Collections;
using System.Collections.Generic;

namespace Redbox.Rental.Model.Personalization;

public class RecommendedTitles : IRecommendedTitles, IList<IRecommendedTitleId>, ICollection<IRecommendedTitleId>, IEnumerable<IRecommendedTitleId>, IEnumerable
{
	public const string NoOfferCode = "NO_OFFER";

	public IRecommendedTitleId this[int index]
	{
		get
		{
			return Ids[index];
		}
		set
		{
			Ids[index] = value;
		}
	}

	public string TivoQueryId { get; set; }

	public string OfferCode { get; set; }

	private IList<IRecommendedTitleId> Ids { get; }

	public int Count => Ids.Count;

	public bool IsReadOnly => false;

	public RecommendedTitles()
	{
		Ids = new List<IRecommendedTitleId>();
	}

	public RecommendedTitles(IList<IRecommendedTitleId> titleIds)
		: this()
	{
		Ids = titleIds;
	}

	public void Add(IRecommendedTitleId item)
	{
		Ids.Add(item);
	}

	public void Clear()
	{
		Ids.Clear();
	}

	public bool Contains(IRecommendedTitleId item)
	{
		return Ids.Contains(item);
	}

	public void CopyTo(IRecommendedTitleId[] array, int arrayIndex)
	{
		Ids.CopyTo(array, arrayIndex);
	}

	public IEnumerator<IRecommendedTitleId> GetEnumerator()
	{
		return Ids.GetEnumerator();
	}

	public int IndexOf(IRecommendedTitleId item)
	{
		return Ids.IndexOf(item);
	}

	public void Insert(int index, IRecommendedTitleId item)
	{
		Ids.Insert(index, item);
	}

	public bool Remove(IRecommendedTitleId item)
	{
		return Ids.Remove(item);
	}

	public void RemoveAt(int index)
	{
		Ids.RemoveAt(index);
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return Ids.GetEnumerator();
	}
}
