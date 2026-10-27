using System.Collections.Generic;
using Redbox.Rental.Model.Browse;

namespace Redbox.Rental.UI.Models;

public delegate void BrowseItemsModelEvent(List<IBrowseItemModel> browseItemModels, object parameter);
