using CykeMaps.UI.Sheets;
using System;
using System.Windows.Input;

namespace CykeMaps.Core.Location.Actions.Commands
{
    public class RemoveFavorite : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is Favorite)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async void Execute(object parameter)
        {
            if (parameter is ILocation && parameter is Favorite)
            {
                await (parameter as Favorite).Delete();
                if (MainPage.MainNavigationManager.SheetFrame.SourcePageType == typeof(LocationSheet))
                {
                    MainPage.MainNavigationManager.NavigateBack();
                }
            }
        }
    }
}
