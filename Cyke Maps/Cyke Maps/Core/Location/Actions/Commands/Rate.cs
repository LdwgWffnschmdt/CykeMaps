﻿using System;
using System.Reflection;
using System.Windows.Input;

namespace CykeMaps.Core.Location.Actions.Commands
{
    public class Rate : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            if (parameter is ILocation)
            {
                PropertyInfo info = parameter.GetType().GetProperty("rating");
                if (info != null && info.GetValue(parameter) != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
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
