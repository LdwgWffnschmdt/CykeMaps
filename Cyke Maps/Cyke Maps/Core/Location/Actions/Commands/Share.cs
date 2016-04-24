using System;
using System.Windows.Input;

namespace CykeMaps.Core.Location.Actions.Commands
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
