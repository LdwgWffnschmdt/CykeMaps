using CykeMaps.Core.Location;
using CykeMaps.UI.Navigation;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CykeMaps.UI.Sheets
{
    /// <summary>
    /// Fav
    /// </summary>
    public sealed partial class AddFavoriteSheet : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private ILocation location;

        public ILocation Location
        {
            get { return location; }
            set
            {
                location = value;
                OnPropertyChanged();
            }
        }

        private string name = "";

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                OnPropertyChanged();
                OnPropertyChanged("Creatable");
            }
        }

        private string address = "";

        public string Address
        {
            get { return address; }
            set
            {
                address = value;
                OnPropertyChanged();
                //OnPropertyChanged("Creatable");
            }
        }

        private string description = "";

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged();
                OnPropertyChanged("Creatable");
            }
        }

        private string symbol = "";

        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                OnPropertyChanged();
                OnPropertyChanged("Creatable");
            }
        }

        private string collection = "";

        public string Collection
        {
            get { return collection; }
            set
            {
                collection = value;
                OnPropertyChanged();
                OnPropertyChanged("Creatable");
            }
        }

        public bool Creatable
        {
            get
            {
                if (name != "" && address != "" && description != "" && symbol != "") return true;
                else return false;
            }
        }


        public AddFavoriteSheet()
        {
            this.InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this; // For Binding
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // We are going to cast the property Parameter of NavigationEventArgs object            
            Location = e.Parameter as ILocation;
            Name = Location.Name;
            Address = Location.Address;
            Description = Location.Description;
            Symbol = Location.Symbol;
        }


        private async void Accept(object sender, RoutedEventArgs e)
        {
            if (!Creatable) return;

            Favorite favorite = new Favorite()
            {
                Name = name,
                Address = address,
                Description = description,
                Symbol = symbol,
                Location = Location.Location,
                Timestamp = DateTime.Now
            };

            await favorite.SaveToFile(collection);

            State favState = new State()
            {
                Sheet = typeof(LocationSheet),
                MapCenter = favorite.Location,
                MapZoomLevel = 14
            };

            NavigationManager.Current.NavigateTo(favState, favorite);
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            NavigationManager.Current.NavigateBack();
        }
    }
}
