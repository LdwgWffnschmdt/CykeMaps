using System;
using System.Windows.Input;

namespace CykeMaps.Core.Location.Actions.Commands
{
    public class ShowOnMap : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return true;
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
            if (parameter is ILocation) MainPage.MainNavigationManager.ShowOnMap(parameter as ILocation);
        }
    }
}
