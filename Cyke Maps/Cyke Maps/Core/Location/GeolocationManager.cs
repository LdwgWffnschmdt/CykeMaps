using CykeMaps.UI;
using CykeMaps.Core.Location;
using System;
using Windows.Devices.Geolocation;
using Windows.Devices.Sensors;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls.Maps;
using Windows.Foundation;
using Windows.ApplicationModel.Core;



namespace CykeMaps.Core.Location
{
    /// <summary>
    /// Thos one tracks the Current Location of the user as well the Compass
    /// </summary>
    public sealed partial class GeolocationManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        // Proides access to location data
        private Geolocator _geolocator = null;

        private Compass _compass;
        private uint _desiredReportInterval;
        CalibrationBar calibrationBar = new CalibrationBar();


        private MapControl MapMain;
        private MapGeolocationPin GeolocationPin;
        private MapPolygon AccuracyCircle;


        #region Fields

        private MapPositionState mapState;

        public MapPositionState MapState
        {
            get { return mapState; }
            set
            {
                mapState = value;

                if (mapState != MapPositionState.None) MapMain.TrySetViewAsync(currentPosition.Point);

                OnPropertyChanged();
            }
        }



        private Geocoordinate currentPosition;

        public Geocoordinate CurrentPosition
        {
            get { return currentPosition; }
            set
            {
                currentPosition = value;

                if (currentPosition == null)
                {
                    GeolocationPin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    AccuracyCircle.Visible = false;
                }
                else
                {
                    MapControl.SetLocation(GeolocationPin, currentPosition.Point);
                    GeolocationPin.Title = currentPosition.Latitude.ToString() + ", " + currentPosition.Longitude.ToString();

                    AccuracyCircle.Path = currentPosition.Point.GetCirclePoints(currentPosition.Accuracy).ToGeoPath();

                    if (mapState != MapPositionState.None)
                    {
                        MapMain.TrySetViewAsync(currentPosition.Point);
                    }
                }

                OnPropertyChanged();
            }
        }

        private PositionStatus positionAccessStatus;

        public PositionStatus PositionAccessStatus
        {
            get { return positionAccessStatus; }
            set
            {
                positionAccessStatus = value;
            }
        }

        private CompassReading heading;

        public CompassReading Heading
        {
            get { return heading; }
            set { heading = value; }
        }


        #endregion


        /// <summary>
        /// This holds the instance to the Only GeolocationManager in this app.
        /// </summary>
        public static GeolocationManager Instance { get; protected set; }

        public GeolocationManager(ref MapControl MapMain)
        {
            // Check is the instance doesnt already exist.
            if (Instance != null)
            {
                //if there is an instance in the app already present then simply throw an error.
                throw new Exception("Only one settings manager can exist in a App.");
            }

            // Setting the instance to the static instance field.
            Instance = this;

            this.MapMain = MapMain;

            GeolocationPin = new MapGeolocationPin();
            MapMain.Children.Add(GeolocationPin);
            MapControl.SetNormalizedAnchorPoint(GeolocationPin, new Point(0.5, 1));
            MapControl.SetLocation(GeolocationPin, MapMain.Center);

            AccuracyCircle = new MapPolygon()
            {
                FillColor = Windows.UI.Color.FromArgb(60, 97, 211, 32),
                StrokeColor = Windows.UI.Color.FromArgb(100, 255, 255, 255)
            };

            MapMain.MapElements.Add(AccuracyCircle);

            // Location tracking location and Compass
            StartTracking();
        }


        /// <summary>
        /// This is the click handler for the 'StartTracking' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async Task StartTracking()
        {
            // Compass
            _compass = Compass.GetDefault();
            if (_compass != null)
            {
                // Select a report interval that is both suitable for the purposes of the app and supported by the sensor.
                // This value will be used later to activate the sensor.
                uint minReportInterval = _compass.MinimumReportInterval;
                _desiredReportInterval = minReportInterval > 16 ? minReportInterval : 16;

                // Establish the report interval
                _compass.ReportInterval = _desiredReportInterval;
                
                _compass.ReadingChanged += new TypedEventHandler<Compass, CompassReadingChangedEventArgs>(ReadingChanged);

                
            }
            else
            {
                //rootPage.NotifyUser("No compass found", NotifyType.ErrorMessage);
            }


            // Request permission to access location
            var accessStatus = await Geolocator.RequestAccessAsync();

            switch (accessStatus)
            {
                case GeolocationAccessStatus.Allowed:
                    // You should set MovementThreshold for distance-based tracking
                    // or ReportInterval for periodic-based tracking before adding event
                    // handlers. If none is set, a ReportInterval of 1 second is used
                    // as a default and a position will be returned every 1 second.
                    //
                    _geolocator = new Geolocator { MovementThreshold = 2, DesiredAccuracyInMeters = 5 };

                    // Subscribe to PositionChanged event to get updated tracking positions
                    _geolocator.PositionChanged += OnPositionChanged;

                    // Subscribe to StatusChanged event to get updates of location status changes
                    _geolocator.StatusChanged += OnStatusChanged;

                    //_rootPage.NotifyUser("Waiting for update...", NotifyType.StatusMessage);


                    await ShowStatus("Suche aktuelle Position...");


                    break;

                case GeolocationAccessStatus.Denied:
                    await ShowStatus("Zugriff auf Position verweigert");
                    break;

                case GeolocationAccessStatus.Unspecified:
                    await ShowStatus("Fehler bei Ortung");
                    break;
            }
        }

        private async Task ShowStatus(string status = "")
        {
            // Handle the StatusBar on Mobile
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();

                if (status == "")
                {
                    await statusBar.ProgressIndicator.HideAsync();
                    statusBar.ProgressIndicator.Text = "";
                }
                else
                {
                    await statusBar.ProgressIndicator.ShowAsync();
                    statusBar.ProgressIndicator.Text = status;
                }
            }

        }

        /// <summary>
        /// This is the click handler for the 'StopTracking' button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void StopTracking()
        {
            // Stop Compass
            //_compass.ReadingChanged -= new TypedEventHandler<Compass, CompassReadingChangedEventArgs>(ReadingChanged);

            // Restore the default report interval to release resources while the sensor is not in use
            //_compass.ReportInterval = 0;


            // Stop location
            _geolocator.PositionChanged -= OnPositionChanged;
            _geolocator.StatusChanged -= OnStatusChanged;
            _geolocator = null;

            // Clear status
            ShowStatus("");
        }

        /// <summary>
        /// Event handler for PositionChanged events. It is raised when
        /// a location is available for the tracking session specified.
        /// </summary>
        /// <param name="sender">Geolocator instance</param>
        /// <param name="e">Position data</param>
        async private void OnPositionChanged(Geolocator sender, PositionChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                if (e.Position == null)
                {
                    ShowStatus("No Data");
                }
                else
                {
                    CurrentPosition = e.Position.Coordinate;
                }
            });
        }

        /// <summary>
        /// Event handler for StatusChanged events. It is raised when the 
        /// location status in the system changes.
        /// </summary>
        /// <param name="sender">Geolocator instance</param>
        /// <param name="e">Statu data</param>
        async private void OnStatusChanged(Geolocator sender, StatusChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                GeolocationPin.Status = e.Status;
                switch (e.Status)
                {
                    case PositionStatus.Ready:
                        // Location platform is providing valid data.
                        ShowStatus();
                        break;

                    case PositionStatus.Initializing:
                        // Location platform is attempting to acquire a fix. 
                        ShowStatus("Initializing");
                        break;

                    case PositionStatus.NoData:
                        // Location platform could not obtain location data.
                        ShowStatus("No data");
                        break;

                    case PositionStatus.Disabled:
                        // The permission to access location data is denied by the user or other policies.
                        ShowStatus("Disabled");

                        // Show message to the user to go to location settings
                        //LocationDisabledMessage.Visibility = Visibility.Visible;

                        // Clear cached location data if any
                        CurrentPosition = null;
                        break;

                    case PositionStatus.NotInitialized:
                        // The location platform is not initialized. This indicates that the application 
                        // has not made a request for location data.
                        ShowStatus("Not initialized");
                        break;

                    case PositionStatus.NotAvailable:
                        // The location platform is not available on this version of the OS.
                        ShowStatus("Not available");
                        break;

                    default:
                        ShowStatus("Unknown");
                        GeolocationPin.Status = PositionStatus.NoData;
                        break;
                }
            });
        }
        
        /// <summary>
        /// This is the event handler for Compass' ReadingChanged events.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        async private void ReadingChanged(object sender, CompassReadingChangedEventArgs e)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                CompassReading reading = e.Reading;

                Heading = e.Reading;

                // Calibrate if needed
                if (reading.HeadingAccuracy != MagnetometerAccuracy.High)
                {
                    calibrationBar.RequestCalibration(reading.HeadingAccuracy);
                }
                
                if (mapState == MapPositionState.FollowAndRotate)
                {
                    GeolocationPin.Heading = 0;
                    MapMain.Heading = (double)reading.HeadingTrueNorth;
                }
                else
                {
                    // This has to be the negative
                    GeolocationPin.Heading = (int)(MapMain.Heading - reading.HeadingTrueNorth.Value);
                }

                /*ScenarioOutput_MagneticNorth.Text = String.Format("{0,5:0.00}", reading.HeadingMagneticNorth);
                if (reading.HeadingTrueNorth != null)
                {
                    ScenarioOutput_TrueNorth.Text = String.Format("{0,5:0.00}", reading.HeadingTrueNorth);
                }
                else
                {
                    ScenarioOutput_TrueNorth.Text = "No data";
                }
                switch (reading.HeadingAccuracy)
                {
                    case MagnetometerAccuracy.Unknown:
                        ScenarioOutput_HeadingAccuracy.Text = "Unknown";
                        break;
                    case MagnetometerAccuracy.Unreliable:
                        ScenarioOutput_HeadingAccuracy.Text = "Unreliable";
                        break;
                    case MagnetometerAccuracy.Approximate:
                        ScenarioOutput_HeadingAccuracy.Text = "Approximate";
                        break;
                    case MagnetometerAccuracy.High:
                        ScenarioOutput_HeadingAccuracy.Text = "High";
                        break;
                    default:
                        ScenarioOutput_HeadingAccuracy.Text = "No data";
                        break;
                }*/
            });
        }

    }
}
