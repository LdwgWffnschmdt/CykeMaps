﻿using CykeMaps.Core.Location;
using CykeMaps.UI.Navigation;
using System;
using System.Windows.Input;


namespace CykeMaps.Core.Actions.Commands
{
    public class EditFavorite : ICommand
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

        public void Execute(object parameter)
        {
            if (parameter is Favorite)
            {
                NavigationManager.Current.EditFavorite((ILocation) parameter);
            }
        }
    }
}
