using CykeMaps.Core.Route;
using CykeMaps.UI.Navigation;
using System;
using System.Windows.Input;

namespace CykeMaps.Core.Actions.Commands
{
    public class SaveRoute : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is FavoriteRoute)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void Execute(object parameter)
        {
            if (parameter is IRoute && !(parameter is FavoriteRoute))
            {
                NavigationManager.Current.SaveRoute((IRoute) parameter);
            }
        }
    }
}
