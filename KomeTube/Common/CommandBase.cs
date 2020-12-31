using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace KomeTube.Common
{
    /// <summary>
    /// The command base used for MVVM design pattern.
    /// </summary>
    public class CommandBase : ICommand
    {
        #region Private Fields

        private readonly Action<object> _command;
        private readonly Func<object, bool> _canExecute;

        #endregion Private Fields

        #region Constructor

        public CommandBase(Action<object> command, Func<object, bool> canExecute = null)
        {
            if (command == null)
                throw new ArgumentNullException("command");
            _canExecute = canExecute;
            _command = command;
        }

        #endregion Constructor

        #region ICommand Members

        public void Execute(object parameter)
        {
            _command(parameter);
        }

        public bool CanExecute(object parameter)
        {
            if (_canExecute == null)
                return true;
            return _canExecute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        #endregion ICommand Members
    }
}