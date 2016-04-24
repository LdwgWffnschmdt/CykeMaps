using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CykeMaps.Core
{

    public class Collection<T> : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private string name;

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string symbol = "Folder";

        public string Symbol
        {
            get { return symbol; }
            set { symbol = value; }
        }



        public ObservableCollection<T> Items { get; set; }

        public Collection(string name, string symbol = "Folder")
        {
            this.name = name;
            this.symbol = symbol;
            this.Items = new ObservableCollection<T>();
        }
    }
}
