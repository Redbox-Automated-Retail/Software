using System;
using System.Windows.Input;

namespace Redbox.Rental.UI;

public class DynamicRoutedCommand : ICommand
{
	private dynamic _doExecute;

	private dynamic _canExecute;

	public event EventHandler CanExecuteChanged
	{
		add
		{
			CommandManager.RequerySuggested += value;
		}
		remove
		{
			CommandManager.RequerySuggested -= value;
		}
	}

	public void InitDynamicExecutes(dynamic doExecute, dynamic canExecute)
	{
		if (doExecute == null)
		{
			throw new NullReferenceException("The doExecute cannot be null.");
		}
		_doExecute = doExecute;
		_canExecute = canExecute;
	}

	public bool CanExecute(dynamic parameter)
	{
		bool result = true;
		if (_canExecute != null)
		{
			result = ((!((parameter == null && _canExecute.GetType() == typeof(Func<bool>)) ? true : false)) ? ((bool)_canExecute.Invoke(parameter)) : ((bool)_canExecute.Invoke()));
		}
		return result;
	}

	public void Execute(dynamic parameter)
	{
		if (parameter == null && _doExecute.GetType() == typeof(Action))
		{
			_doExecute.Invoke();
		}
		else
		{
			_doExecute.Invoke(parameter);
		}
	}
}
