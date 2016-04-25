using CykeMaps.Core.Location;
using CykeMaps.Core.Route;
using System;
using System.Windows.Input;

namespace CykeMaps.Core.Actions.Commands
{
    public class Share : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is ILocation)
            {
                // Why should it not be sharable?
                return true;
            }
            else if (parameter is IRoute)
            {
                // Why should it not be sharable?
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
