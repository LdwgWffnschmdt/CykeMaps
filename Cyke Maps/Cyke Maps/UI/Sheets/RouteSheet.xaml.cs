using CykeMaps.Core.Actions;
using CykeMaps.Core.Location;
using CykeMaps.Core.Route;
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
    /// A sheet to display a Route
    /// </summary>
    public sealed partial class RouteSheet : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private IRoute routeBuffer = new BasicRoute();
        private string collectionBuffer;

        public RouteSheet()
        {

            this.InitializeComponent();
            LoadCollectionList();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // We are going to cast the property Parameter of NavigationEventArgs object            
            Route = e.Parameter as IRoute;
            Route.PropertyChanged += Route_PropertyChanged;

            RootGrid.DataContext = this as RouteSheet;


            Windows.UI.Xaml.Controls.Maps.MapPolyline line = new Windows.UI.Xaml.Controls.Maps.MapPolyline();
            line.Path = new Windows.Devices.Geolocation.Geopath(Route.Track);

            MainPage.Current.MapMain.MapElements.Add(line);


            if (Route != null) CreateAppBarButtons();
        }

        #region Fields

        private IRoute route;

        public IRoute Route
        {
            get { return route; }
            set
            {
                route = value;
                
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
                if ( Route.Name != null && Route.Name.Length > 0) return true;
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
            StorageFolder FavoritesFolder = await ApplicationData.Current.RoamingFolder.CreateFolderAsync("Routes", CreationCollisionOption.OpenIfExists);
            CollectionList = from x in (await FavoritesFolder.GetFoldersAsync())
                             select x.DisplayName;
        }


        private void Route_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Route");
            OnPropertyChanged("Creatable");
        }

        private void CreateAppBarButtons()
        {
            RouteCommandBar.PrimaryCommands.Clear();
            RouteCommandBar.SecondaryCommands.Clear();

            foreach (IAction action in Route.Actions)
            {
                AppBarButton locationSheetAppBarButton = new AppBarButton();
                locationSheetAppBarButton.Icon = new SymbolIcon((Symbol)Enum.Parse(typeof(Symbol), action.Symbol, true));
                locationSheetAppBarButton.Label = action.Text;
                locationSheetAppBarButton.Command = action.Action;
                locationSheetAppBarButton.CommandParameter = Route;
                RouteCommandBar.PrimaryCommands.Add(locationSheetAppBarButton);
            }

            foreach (IAction action in Route.SecondaryActions)
            {
                AppBarButton locationSheetAppBarButton = new AppBarButton();
                locationSheetAppBarButton.Icon = new SymbolIcon((Symbol)Enum.Parse(typeof(Symbol), action.Symbol, true));
                locationSheetAppBarButton.Label = action.Text;
                locationSheetAppBarButton.Command = action.Action;
                locationSheetAppBarButton.CommandParameter = Route;
                RouteCommandBar.SecondaryCommands.Add(locationSheetAppBarButton);
            }
        }

        public void AddEditFavorite(object sender, RoutedEventArgs e)
        {
            // Save the original parameters in the Buffer --> Restore if canceled
            routeBuffer.Name = route.Name;
            routeBuffer.Description = route.Description;
            routeBuffer.Symbol = route.Symbol;

            if (route is FavoriteRoute) collection = (route as FavoriteRoute).Collection;
            collectionBuffer = collection;

            VisualStateManager.GoToState(this, "AddEditFavoriteState", true);
        }

        private async void Accept(object sender, RoutedEventArgs e)
        {
            if (!Creatable) return;

            FavoriteRoute favorite = new FavoriteRoute()
            {
                Name = route.Name,
                Description = route.Description,
                Downhill = route.Downhill,
                Uphill = route.Uphill,
                Track = route.Track,
                Distance = route.Distance,
                BoundingBox = route.BoundingBox,
                StartPoint = route.StartPoint,
                Collection = collection,
                Symbol = route.Symbol,
                Timestamp = DateTime.Now
            };

            // Its a new collection name, delete it from the old folder ...
            if (route is FavoriteRoute && collection != collectionBuffer) await (route as FavoriteRoute).Delete();

            // ... and create a new Favorite in the right collection folder
            await favorite.SaveToFile(collection);

            // Set this as the new location
            Route = favorite;

            // Reset the Buffer
            routeBuffer = route;

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
            Route.Name = routeBuffer.Name;
            Route.Description = routeBuffer.Description;
            Route.Symbol = routeBuffer.Symbol;
            Collection = collectionBuffer;

            VisualStateManager.GoToState(this, "Default", true);
        }
    }
}
