namespace Redbox.Rental.UI.Controls;

public class PageButtonInfo : ItemInfo
{
	private bool _isCurrentPage;

	private bool _isNotCurrentPage;

	public int PageNumber { get; set; }

	public bool IsCurrentPage
	{
		get
		{
			return _isCurrentPage;
		}
		set
		{
			_isCurrentPage = value;
			_isNotCurrentPage = !value;
		}
	}

	public bool IsNotCurrentPage
	{
		get
		{
			return _isNotCurrentPage;
		}
		set
		{
			_isNotCurrentPage = value;
			_isCurrentPage = !value;
		}
	}
}
