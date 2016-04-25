using System.Windows.Input;

namespace CykeMaps.Core.Actions
{
    public class BasicAction : IAction
    {
        private string symbol;
        private string text;
        private ICommand action;

        public BasicAction(string _Symbol, string _Text, ICommand _Action)
        {
            symbol = _Symbol;
            text = _Text;
            action = _Action;
        }

        public string Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }

        public string Text
        {
            get { return text; }
            set { text = value; }
        }

        public ICommand Action
        {
            get { return action; }
            set { action = value; }
        }
    }
}
