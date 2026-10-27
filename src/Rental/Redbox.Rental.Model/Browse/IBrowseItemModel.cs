namespace Redbox.Rental.Model.Browse;

public interface IBrowseItemModel
{
	string Name { get; set; }

	object Data { get; set; }

	bool IsBorderItem { get; set; }

	bool IsBottomRow { get; set; }

	int VisibleItemIndex { get; set; }

	bool CanAdd { get; set; }

	int CurrentGridRow { get; set; }

	int CurrentGridColumn { get; set; }
}
