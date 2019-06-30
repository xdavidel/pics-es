using System;
using System.Windows.Input;

namespace PicsEs.Helpers
{
	public class DelegateCommand : ICommand
	{
		private Action<object> _execute;
		private Predicate<object> _canExecute;

		public event EventHandler CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public DelegateCommand(Action<object> execute, Predicate<object> canExecute = null)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute != null ? _canExecute(parameter) : true;
		}

		public void Execute(object parameter)
		{
			_execute?.Invoke(parameter);
		}
	}
}
