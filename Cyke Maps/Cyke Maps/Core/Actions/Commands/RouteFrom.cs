using CykeMaps.Core.Location;
using System;
using System.Windows.Input;

namespace CykeMaps.Core.Actions.Commands
{
    public class RouteFrom : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is ILocation)
            {
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
