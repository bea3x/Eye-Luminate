using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RecorderApp.Utility
{
    public sealed class RelayCommand : ICommand
    {
        private Action function;

        public Action<object> P1 { get; }
        public Func<object, bool> P2 { get; }
        public Func<object, object> P { get; }

        public RelayCommand(Action function)
        {
            this.function = function;
        }
        public RelayCommand(Action<object> p1)
        {
            P1 = p1;
        }
        public RelayCommand(Action<object> p1, Func<object, bool> p2)
        {
            P1 = p1;
            P2 = p2;
        }

        public RelayCommand(Action<object> p1, Func<object, bool> p2, Func<object, object> p) : this(p1, p2)
        {
            P = p;
        }

        public bool CanExecute(object parameter)
        {
            return this.function != null;
        }

        public void Execute(object parameter)
        {
            if (this.function != null)
            {
                this.function();
            }
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
