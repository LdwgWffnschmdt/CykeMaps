using CykeMaps.Core.Location;
using CykeMaps.Core.Route;
using System;
using System.Windows.Input;

namespace CykeMaps.Core.Actions.Commands
{
    public class UnPinFromStart : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is ILocation || parameter is IRoute)
            {
                // TODO: CHECK IF IT IS PINNED
                return true;
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
