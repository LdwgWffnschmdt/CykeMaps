using CykeMaps.Core.Location;
using CykeMaps.Core.Route;
using CykeMaps.Core.Route.RouteRequest;
using CykeMaps.UI.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
    /// A sheet to display a RouteRequest
    /// </summary>
    public sealed partial class RouteRequestSheet : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        public RouteRequestSheet()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // We are going to cast the property Parameter of NavigationEventArgs object            
            RouteRequest = e.Parameter as IRouteRequest;
            RouteRequest.PropertyChanged += RouteRequest_PropertyChanged;

            RootGrid.DataContext = this as RouteRequestSheet;
        }

        #region Fields

        private IRouteRequest routeRequest;

        public IRouteRequest RouteRequest
        {
            get { return routeRequest; }
            set
            {
                routeRequest = value;
                
                OnPropertyChanged();
                OnPropertyChanged("Creatable");
            }
        }
        
        #endregion
        
        private void RouteRequest_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged("Location");
            OnPropertyChanged("Creatable");
        }
        
        private async void CalculateRoute(object sender, RoutedEventArgs e)
        {
            if (!RouteRequest.Sendable) return;

            VisualStateManager.GoToState(this, "Calculating", true);

            IRoute Route = await RouteRequest.GetRoute();

            Debug.WriteLine(Route.Distance);

            VisualStateManager.GoToState(this, "Default", true);

            NavigationManager.Current.ShowRoute(Route);
        }
    }
}
