using System;
using System.Windows.Input;

namespace CykeMaps.Core.Location.Actions.Commands
{
    public class PinToStart : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is ILocation)
            {
                // CHECK IF IT IS ALREADY PINNED
                return false;
            }
            else
            {
                return false;
            }
        }

        public void Execute(object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
