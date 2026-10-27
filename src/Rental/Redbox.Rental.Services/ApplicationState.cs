using System.Collections.Generic;
using System.Linq;
using Redbox.Core;
using Redbox.KioskEngine.ComponentModel;
using Redbox.Rental.Model;
using Redbox.Rental.Model.KioskProduct;

namespace Redbox.Rental.Services;

public class ApplicationState : IApplicationState
{
	public delegate void OnRemoveMaintenanceModeReason(IMaintenanceModeReason removedMaintenanceModeReason, List<IMaintenanceModeReason> remainingMaintenanceModeReasons);

	public delegate void OnAddMaintenanceModeReason(IMaintenanceModeReason addedMaintenanceModeReason);

	private class MaintenanceModeReason : IMaintenanceModeReason
	{
		public MaintenanceModeSource Source { get; set; }

		public string Code { get; set; }
	}

	private readonly object _lockObject = new object();

	private readonly List<IMaintenanceModeReason> _reasonsForMaintenanceMode = new List<IMaintenanceModeReason>();

	private bool _needToReloadCarousel;

	private string _inCartTemplateName;

	private bool _isADAAccessible;

	private static ApplicationState _applicationState;

	public static ApplicationState Instance
	{
		get
		{
			if (_applicationState == null)
			{
				_applicationState = new ApplicationState();
				ServiceLocator.Instance.AddService(typeof(IApplicationState), _applicationState);
			}
			return _applicationState;
		}
	}

	public bool IsADAAccessible
	{
		get
		{
			return _isADAAccessible;
		}
		set
		{
			_isADAAccessible = value;
		}
	}

	public bool IsABEOn
	{
		get
		{
			ITextToSpeechService service = ServiceLocator.Instance.GetService<ITextToSpeechService>();
			if (!IsADAAccessible)
			{
				return service?.TTSEnabled ?? false;
			}
			return true;
		}
	}

	public string InCartTemplateName
	{
		get
		{
			return _inCartTemplateName;
		}
		set
		{
			_inCartTemplateName = value;
		}
	}

	public bool VerboseLogging
	{
		get
		{
			bool.TryParse(ServiceLocator.Instance.GetService<IMachineSettingsStore>()?.GetValue("Store", "VerboseLogging", "False"), out var result);
			return result;
		}
	}

	public List<IKioskProduct> BrowseProducts { get; set; }

	public bool IsStandAlone
	{
		get
		{
			return ServiceLocator.Instance.GetService<IMachineSettingsStore>().GetValue("Store", "StandAlone", defaultValue: false);
		}
		set
		{
			ServiceLocator.Instance.GetService<IMachineSettingsStore>().SetValue("Store", "StandAlone", value);
		}
	}

	public bool NeedToReloadCarousel
	{
		get
		{
			return _needToReloadCarousel;
		}
		set
		{
			_needToReloadCarousel = value;
		}
	}

	public bool IsInMaintenanceMode
	{
		get
		{
			lock (_lockObject)
			{
				return _reasonsForMaintenanceMode?.Any() ?? false;
			}
		}
	}

	public bool HasMinutesInMaintenanceModeSource
	{
		get
		{
			lock (_lockObject)
			{
				return _reasonsForMaintenanceMode.Any((IMaintenanceModeReason x) => x.Source == MaintenanceModeSource.CardReader || x.Source == MaintenanceModeSource.Database || x.Source == MaintenanceModeSource.HAL);
			}
		}
	}

	public event OnRemoveMaintenanceModeReason OnRemoveMaintenanceModeReasonHandler;

	public event OnAddMaintenanceModeReason OnAddMaintenanceModeReasonHandler;

	public bool HasMaintenanceModeReason(MaintenanceModeSource source, string code)
	{
		return _reasonsForMaintenanceMode?.Any((IMaintenanceModeReason x) => x.Source == source && x.Code == code) ?? false;
	}

	public List<IMaintenanceModeReason> GetMaintenanceModeReasons()
	{
		List<IMaintenanceModeReason> list = new List<IMaintenanceModeReason>();
		if (_reasonsForMaintenanceMode != null)
		{
			foreach (IMaintenanceModeReason item in _reasonsForMaintenanceMode)
			{
				list.Add(new MaintenanceModeReason
				{
					Source = item.Source,
					Code = item.Code
				});
			}
		}
		return list;
	}

	public void AddMaintenanceModeReason(MaintenanceModeSource source, string code)
	{
		lock (_lockObject)
		{
			IMaintenanceModeReason maintenanceModeReason = new MaintenanceModeReason
			{
				Source = source,
				Code = code
			};
			if (!_reasonsForMaintenanceMode.Any((IMaintenanceModeReason x) => x.Source == source && x.Code == code))
			{
				_reasonsForMaintenanceMode.Add(maintenanceModeReason);
				LogHelper.Instance.Log($"Adding reason for maintenance mode: Source: {source}, Code: {code}");
				OnAddMaintenanceModeReasonHandler?.Invoke(maintenanceModeReason);
			}
		}
	}

	public void RemoveMaintenanceModeReason(MaintenanceModeSource source, string code)
	{
		lock (_lockObject)
		{
			IMaintenanceModeReason reasonForMaintenanceMode = _reasonsForMaintenanceMode.FirstOrDefault((IMaintenanceModeReason x) => x.Source == source && x.Code == code);
			RemoveMaintenanceModeReason(reasonForMaintenanceMode);
		}
	}

	public void RemoveMaintenanceModeReason(MaintenanceModeSource source)
	{
		lock (_lockObject)
		{
			_reasonsForMaintenanceMode.Where((IMaintenanceModeReason x) => x.Source == source).ToList().ForEach(delegate(IMaintenanceModeReason x)
			{
				RemoveMaintenanceModeReason(x);
			});
		}
	}

	public void ClearOnRemoveMaintenanceModeReasonHandler()
	{
		OnRemoveMaintenanceModeReasonHandler = null;
	}

	public void ClearOnAddMaintenanceModeReasonHandler()
	{
		OnAddMaintenanceModeReasonHandler = null;
	}

	private void RemoveMaintenanceModeReason(IMaintenanceModeReason reasonForMaintenanceMode)
	{
		if (reasonForMaintenanceMode != null)
		{
			lock (_lockObject)
			{
				_reasonsForMaintenanceMode.Remove(reasonForMaintenanceMode);
				LogHelper.Instance.Log($"Removing maintenance mode reason: Source {reasonForMaintenanceMode?.Source}, Code {reasonForMaintenanceMode?.Code}");
				OnRemoveMaintenanceModeReasonHandler?.Invoke(reasonForMaintenanceMode, GetMaintenanceModeReasons());
			}
		}
	}
}
