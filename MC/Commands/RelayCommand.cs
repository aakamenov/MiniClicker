using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MC.Commands
{
    public class RelayCommand<TArgs> : ICommand where TArgs : EventArgs
    {
        readonly Action<TArgs> execute;
        readonly Predicate<object> canExecute;

        public RelayCommand(Action<TArgs> execute) : this(execute, null) { }

        public RelayCommand(Action<TArgs> execute, Predicate<object> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameters)
        {
            return canExecute == null ? true : canExecute(parameters);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameters)
        {
            var castParameter = (TArgs)Convert.ChangeType(parameters, typeof(TArgs));
            execute(castParameter);
        }
    }
}
