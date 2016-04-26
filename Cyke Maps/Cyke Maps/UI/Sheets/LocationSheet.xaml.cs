using CykeMaps.Core.Actions;
using CykeMaps.Core.Location;
using CykeMaps.UI.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CykeMaps.UI.Sheets
{
    /// <summary>
    /// A sheet to display a Location
    /// </summary>
    public sealed partial class LocationSheet : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

            if (propertyName == "Location")
            {
                if (location != null && location is GeocodingLocation)
                {
                    GeocodingLocation geoLoc = location as GeocodingLocation;

                    if (geoLoc.Status == null) GeocodingProgress.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    else GeocodingProgress.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
        }

        private ILocation locationBuffer = new BasicLocation();
        private string collectionBuffer;

        public LocationSheet()
        {

            this.InitializeComponent();
            LoadCollectionList();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // We are going to cast the property Parameter of NavigationEventArgs object            
            Location = e.Parameter as ILocation;
            Location.PropertyChanged += Location_PropertyChanged;

            RootGrid.DataContext = this as LocationSheet;

            if (Location != null) CreateAppBarButtons();
        }

        #region Fields

        private ILocation location;

        public ILocation Location
        {
            get { return location; }
            set
            {
                location = value;
                
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
                if ( Location.Name != null && Location.Name.Length > 0 && 
                     Location.Address != null && Location.Address.Length > 0 &&
                     Location.Symbol != null && Location.Symbol.Length > 0) return true;
                else return false;
            }
        }

        private IEnumerable<string> collectionList;

        public IEnumerable<string> CollectionList
        {
            get { return collectionList; }
            protected set
            {
                collectionList = value;
                OnPropertyChanged();
            }
        }


        private IEnumerable symbolList = Enum.GetNames(typeof(Windows.UI.Xaml.Controls.Symbol));

        public IEnumerable SymbolList
        {
            get { return symbolList; }
            protected set
            {
                symbolList = value;
                OnPropertyChanged();
            }
        }

        #endregion

        private async Task LoadCollectionList()
        {
            StorageFolder FavoritesFolder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Favorites", CreationCollisionOption.OpenIfExists);
            CollectionList = from x in (await FavoritesFolder.GetFoldersAsync())
                             select x.DisplayName;
        }


        private void Location_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Location");
            OnPropertyChanged("Creatable");
        }

        private void CreateAppBarButtons()
        {
            LocationCommandBar.PrimaryCommands.Clear();
            LocationCommandBar.SecondaryCommands.Clear();

            foreach (IAction action in Location.Actions)
            {
                AppBarButton locationSheetAppBarButton = new AppBarButton();
                locationSheetAppBarButton.Icon = new SymbolIcon((Symbol)Enum.Parse(typeof(Symbol), action.Symbol, true));
                locationSheetAppBarButton.Label = action.Text;
                locationSheetAppBarButton.Command = action.Action;
                locationSheetAppBarButton.CommandParameter = Location;
                LocationCommandBar.PrimaryCommands.Add(locationSheetAppBarButton);
            }

            foreach (IAction action in Location.SecondaryActions)
            {
                AppBarButton locationSheetAppBarButton = new AppBarButton();
                locationSheetAppBarButton.Icon = new SymbolIcon((Symbol)Enum.Parse(typeof(Symbol), action.Symbol, true));
                locationSheetAppBarButton.Label = action.Text;
                locationSheetAppBarButton.Command = action.Action;
                locationSheetAppBarButton.CommandParameter = Location;
                LocationCommandBar.SecondaryCommands.Add(locationSheetAppBarButton);
            }
        }

        public void AddEditFavorite(object sender, RoutedEventArgs e)
        {
            // Save the original parameters in the Buffer --> Restore if canceled
            locationBuffer.Name = location.Name;
            locationBuffer.Description = location.Description;
            locationBuffer.Address = location.Address;
            locationBuffer.Symbol = location.Symbol;

            if (location is Favorite) collection = (location as Favorite).Collection;
            collectionBuffer = collection;

            VisualStateManager.GoToState(this, "AddEditFavoriteState", true);
        }

        private async void Accept(object sender, RoutedEventArgs e)
        {
            if (!Creatable) return;

            Favorite favorite = new Favorite()
            {
                Name = location.Name,
                Address = location.Address,
                Description = location.Description,
                Collection = collection,
                Symbol = location.Symbol,
                Location = Location.Location,
                Timestamp = DateTime.Now
            };

            // Its a new collection name, delete it from the old folder ...
            if (location is Favorite && collection != collectionBuffer) await (location as Favorite).Delete();

            // ... and create a new Favorite in the right collection folder
            await favorite.SaveToFile(collection);

            // Set this as the new location
            Location = favorite;

            // Reset the Buffer
            locationBuffer = location;

            // Recreate the CommandBar
            CreateAppBarButtons();

            //VisualStateManager.GoToState(this, "Default", true);

            // Because we added a State with an OnGoBackAction for this
            NavigationManager.Current.NavigateBack();
        }

        private void Cancel(object sender, RoutedEventArgs e)
        {
            CancelAddEditFavorite();

            // Because we added a State with an OnGoBackAction for this
            NavigationManager.Current.NavigateBack();
        }

        public void CancelAddEditFavorite()
        {
            Location.Name = locationBuffer.Name;
            Location.Description = locationBuffer.Description;
            Location.Address = locationBuffer.Address;
            Location.Symbol = locationBuffer.Symbol;
            Collection = collectionBuffer;

            VisualStateManager.GoToState(this, "Default", true);
        }
    }
}
