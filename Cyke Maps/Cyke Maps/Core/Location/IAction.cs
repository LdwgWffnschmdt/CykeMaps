using System.Windows.Input;

namespace CykeMaps.Core.Location
{
    public interface IAction
    {
        string Symbol { get; set; }

        string Text { get; set; }

        ICommand Action { get; set; }
    }
}
