using CykeMaps.Core.Location;
using CykeMaps.Core.Route;
using CykeMaps.UI.Navigation;
using System;
using System.Windows.Input;

namespace CykeMaps.Core.Actions.Commands
{
    public class ShowOnMap : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
            if (parameter is ILocation || parameter is IRoute)
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
            if (parameter is ILocation) NavigationManager.Current.ShowOnMap(parameter as ILocation);
            if (parameter is IRoute) NavigationManager.Current.ShowOnMap(parameter as IRoute);
        }
    }
}
