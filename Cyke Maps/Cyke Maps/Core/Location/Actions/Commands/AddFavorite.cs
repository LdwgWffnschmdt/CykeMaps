using System;
using System.Windows.Input;

namespace CykeMaps.Core.Location.Actions.Commands
{
    public class AddFavorite : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is Favorite)
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
            if (parameter is ILocation && !(parameter is Favorite))
            {
                MainPage.MainNavigationManager.AddFavorite((ILocation) parameter);
            }
        }
    }
}
