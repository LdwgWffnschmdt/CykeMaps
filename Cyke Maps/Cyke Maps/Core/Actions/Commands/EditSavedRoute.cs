using CykeMaps.Core.Route;
using CykeMaps.UI.Navigation;
using System;
using System.Windows.Input;


namespace CykeMaps.Core.Actions.Commands
{
    public class EditSavedRoute : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is FavoriteRoute)
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
            if (parameter is FavoriteRoute)
            {
                NavigationManager.Current.EditSavedRoute((IRoute) parameter);
            }
        }
    }
}
