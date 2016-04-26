using CykeMaps.Core.Location;
using CykeMaps.UI.Navigation;
using System;
using System.Windows.Input;

namespace CykeMaps.Core.Actions.Commands
{
    public class RouteTo : ICommand
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
            if (parameter is ILocation) NavigationManager.Current.RouteTo(parameter as ILocation);
        }
    }
}
