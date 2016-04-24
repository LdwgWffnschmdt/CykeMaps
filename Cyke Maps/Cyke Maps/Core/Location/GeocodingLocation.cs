using System;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Services.Maps;
using Windows.UI.Core;
using Windows.ApplicationModel.Core;


namespace CykeMaps.Core.Location
{
    public class GeocodingLocation : BasicLocation
    {
        #region Constructors

        private string NameDefault = "Suche Position ...";
        private string DescriptionDefault = "";

        public GeocodingLocation(Geopoint Location)
        {
            this.Location = Location;

            Name = NameDefault;
            Description = DescriptionDefault;
            Status = null;

            // Start the Geocoding request
            IAsyncOperation<MapLocationFinderResult> request = MapLocationFinder.FindLocationsAtAsync(Location);

            // Get notified when it is ready
            request.Completed = GeocodingCompleted;
        }

        #endregion

        #region Fields


        private MapLocationFinderStatus? status;

        public MapLocationFinderStatus? Status
        {
            get { return status; }
            set
            {
                status = value;
                OnPropertyChanged();
            }
        }



        #endregion

        async private void GeocodingCompleted(IAsyncOperation<MapLocationFinderResult> asyncInfo, AsyncStatus asyncStatus)
        {
            // Get the result
            MapLocationFinderResult result = asyncInfo.GetResults();

            // 
            // Update the UI thread by using the UI core dispatcher.
            // 
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Update the status
                Status = result.Status;

                // Update th Address
                Address = result.Locations[0].Address.FormattedAddress;

                // If there are Name and/or description provided and they are not set yet, set them!
                if (Name == NameDefault && result.Locations[0].DisplayName != null && result.Locations[0].DisplayName != "") Name = result.Locations[0].DisplayName;
                if (Description == DescriptionDefault && result.Locations[0].Description != null && result.Locations[0].Description != "") Description = result.Locations[0].Description;

                // If the Name is still empty, use the Address
                if (Name == NameDefault || Name == "") Name = Address;
            });
        }



    }
}
