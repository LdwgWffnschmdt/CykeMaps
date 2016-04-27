using CykeMaps.Core;
using CykeMaps.Core.Location;
using CykeMaps.UI;
using CykeMaps.UI.Navigation;
using CykeMaps.UI.Sheets;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Sensors;
using System.Linq;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Data;
using Windows.ApplicationModel.Core;
using System.Threading.Tasks;
using System.Collections.Generic;
using Windows.System.Threading;

namespace CykeMaps
{
    /// <summary>
    /// MainPage
    /// </summary>
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        
        private CollectionViewSource MapElements = new CollectionViewSource();

        private StatusBar statusBar;

        /// <summary>
        /// This holds the instance to the Only MainPage in this app.
        /// </summary>
        public static MainPage Current { get; protected set; }

        public MainPage()
        {
            // Check is the instance doesnt already exist.
            if (Current != null)
            {
                //if there is an instance in the app already present then simply throw an error.
                throw new Exception("Only one MainPage can exist in a App.");
            }

            // This is a static public property that allows downstream pages to get a handle to the MainPage instance
            // in order to call methods that are in this class.
            Current = this;

            // Create the LibraryManager
            new LibraryManager();
            LibraryManager.Current.PropertyChanged += LibraryManager_PropertyChanged;

            MapElements.IsSourceGrouped = false;

            this.InitializeComponent();
            (this.Content as FrameworkElement).DataContext = this;
            
            ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            
            SearchBox.DataContext = LibraryManager.Current;

            /////////////////////////////////////////////////////////// NAVIGATION MANAGER INITIALIZATION ////////////////////////////////
            new NavigationManager(ref SheetsFrame, ref scrollViewer );
            NavigationManager.Current.PropertyChanged += NavigationManager_PropertyChanged;
            
            // Create the SettingsManager
            new SettingsManager(ref MapMain);
            this.RequestedTheme = SettingsManager.Current.AppTheme;
            SettingsManager.Current.PropertyChanged += SettingsManager_PropertyChanged;

            MapInteractionButtons.DataContext = SettingsManager.Current;

            #region TitleBar / StatusBar

            //PC customization
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
            {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null)
                {
                    titleBar.BackgroundColor = Color.FromArgb(255, 30, 30, 30);
                    titleBar.ButtonBackgroundColor = titleBar.BackgroundColor;

                    titleBar.InactiveBackgroundColor = Colors.Black;
                    titleBar.ButtonInactiveBackgroundColor = titleBar.InactiveBackgroundColor;

                    titleBar.ForegroundColor = Colors.White;
                    titleBar.InactiveForegroundColor = titleBar.ForegroundColor;
                    titleBar.ButtonForegroundColor = titleBar.ForegroundColor;
                    titleBar.ButtonHoverForegroundColor = titleBar.ForegroundColor;
                    titleBar.ButtonInactiveForegroundColor = titleBar.ForegroundColor;
                    titleBar.ButtonPressedForegroundColor = titleBar.ForegroundColor;

                    titleBar.ButtonHoverBackgroundColor = Color.FromArgb(255, 60, 60, 60);
                    titleBar.ButtonPressedBackgroundColor = Color.FromArgb(255,80, 80,80);
                }
            }

            // Handle the StatusBar on Mobile
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                statusBar = StatusBar.GetForCurrentView();

                statusBar.BackgroundColor = Colors.Transparent;
                statusBar.ForegroundColor = Colors.Black;
            }

            #endregion
            
            SearchBox.GotFocus += SearchBox_GotFocus;
            SearchBox.LostFocus += SearchBox_LostFocus;

            // Center the map to the last position.
            MapMain.Center = new Geopoint(new BasicGeoposition() {
                Latitude = SettingsManager.Current.LastMapCenter.X,
                Longitude = SettingsManager.Current.LastMapCenter.Y
            });
            MapMain.ZoomLevel = SettingsManager.Current.LastMapZoom;
            MapMain.Heading = SettingsManager.Current.LastMapHeading;
            MapMain.DesiredPitch = SettingsManager.Current.LastMapPitch;

            MapMain.ZoomInteractionMode = SettingsManager.Current.ShowZoomControl ? MapInteractionMode.GestureAndControl : MapInteractionMode.GestureOnly;
            MapMain.RotateInteractionMode = SettingsManager.Current.ShowRotationControl ? MapInteractionMode.GestureAndControl : MapInteractionMode.GestureOnly;
            MapMain.TiltInteractionMode = SettingsManager.Current.ShowTiltControl ? MapInteractionMode.GestureAndControl : MapInteractionMode.GestureOnly;



            GeolocationPin = new MapGeolocationPin();
            MapMain.Children.Add(GeolocationPin);
            MapControl.SetNormalizedAnchorPoint(GeolocationPin, new Point(0.5, 1));
            MapControl.SetLocation(GeolocationPin, SettingsManager.Current.LastGeoPosition.ToGeopoint());

            AccuracyCircle = new MapPolygon()
            {
                FillColor = Windows.UI.Color.FromArgb(60, 97, 211, 32),
                StrokeColor = Windows.UI.Color.FromArgb(100, 255, 255, 255)
            };

            MapMain.MapElements.Add(AccuracyCircle);

            // Location tracking location and Compass
            StartTracking();


            SetSnapPoints();
        }

        private void SettingsManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "AppTheme")
            {
                this.RequestedTheme = SettingsManager.Current.AppTheme;
            }
        }

        private void LibraryManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MapElements.Source = new ObservableCollection<Favorite>(LibraryManager.Current.FilteredLibrary.Items[0].Items.SelectMany(b => b.Items
                                                                                                                .Select(a => a as Favorite)));
        }

        private void NavigationManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SharedState")
            {
                if     (NavigationManager.Current.SharedState.HasMapCenter &&
                        NavigationManager.Current.SharedState.HasMapZoomLevel &&
                        NavigationManager.Current.SharedState.HasMapHeading &&
                        NavigationManager.Current.SharedState.HasMapPitch)
                            MapMain.TrySetViewAsync(NavigationManager.Current.SharedState.MapCenter,
                                                    NavigationManager.Current.SharedState.MapZoomLevel,
                                                    NavigationManager.Current.SharedState.MapHeading,
                                                    NavigationManager.Current.SharedState.MapPitch);
                else if (NavigationManager.Current.SharedState.HasMapCenter &&
                         NavigationManager.Current.SharedState.HasMapZoomLevel &&
                         NavigationManager.Current.SharedState.HasMapPitch)
                            MapMain.TrySetViewAsync(NavigationManager.Current.SharedState.MapCenter,
                                                    NavigationManager.Current.SharedState.MapZoomLevel,
                                                    NavigationManager.Current.SharedState.MapHeading,
                                                    null);
                else if (NavigationManager.Current.SharedState.HasMapCenter &&
                         NavigationManager.Current.SharedState.HasMapZoomLevel)
                            MapMain.TrySetViewAsync(NavigationManager.Current.SharedState.MapCenter,
                                                    NavigationManager.Current.SharedState.MapZoomLevel);

                else if (NavigationManager.Current.SharedState.HasMapCenter)
                            MapMain.TrySetViewAsync(NavigationManager.Current.SharedState.MapCenter);


                if (NavigationManager.Current.SharedState.HasMapElements)
                {
                    // TODO: Ensure the map shows the elements
                    /*BasicGeoposition Position1 = new BasicGeoposition();

                    BasicGeoposition Position2 = new BasicGeoposition();

                    foreach (MapElement element in NavigationManager.Current.SharedState.MapElements)
                    {
                        BasicGeoposition elementPosition = MapControl.GetLocation(element).Position;
                        if (Position1.Latitude < elementPosition.Latitude) ;

                    }

                    GeoboundingBox BBox = new GeoboundingBox(Position1, Position1);
                    BBox.
                    MapMain.TrySetViewBoundsAsync()*/
                }


                if (NavigationManager.Current.SharedState.SheetVisibility != UI.Navigation.Visibility.Full)
                {
                    MapMain.ZoomInteractionMode = SettingsManager.Current.ShowZoomControl ? MapInteractionMode.GestureAndControl : MapInteractionMode.GestureOnly;
                    MapMain.RotateInteractionMode = SettingsManager.Current.ShowRotationControl ? MapInteractionMode.GestureAndControl : MapInteractionMode.GestureOnly;
                    MapMain.TiltInteractionMode = SettingsManager.Current.ShowTiltControl ? MapInteractionMode.GestureAndControl : MapInteractionMode.GestureOnly;
                }
                else
                {
                    MapMain.ZoomInteractionMode = MapInteractionMode.GestureOnly;
                    MapMain.RotateInteractionMode = MapInteractionMode.GestureOnly;
                    MapMain.TiltInteractionMode = MapInteractionMode.GestureOnly;
                }
                

                if (SheetsFrame.SourcePageType == typeof(LibrarySheet))
                {
                    VisualStateManager.GoToState(this, "Full", true);
                }
                else
                {
                    VisualStateManager.GoToState(this, "Half", true);
                }

                SetSnapPoints();
            }
        }

        #region Navigation

        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SheetsFrame.CurrentSourcePageType == typeof(LibrarySheet))
            {
                NavigationManager.Current.NavigateTo(NavigationManager.Current.VisibilityFull, null);
            }
            else
            {
                State SearchState = new State()
                {
                    Sheet = typeof(LibrarySheet),
                    SheetVisibility = UI.Navigation.Visibility.Full
                };
                NavigationManager.Current.NavigateTo(SearchState, null);
            }
        }

        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            /*if (SearchBox.Text == "") // Only if the user didn't search for anything --> Go back to normal
            {
                NavigationManager.Current.NavigateTo(NavigationManager.Current.VisibilityHidden, null);
            }*/
        }

        #endregion

        #region Map

        private MapPositionState mapState;

        public MapPositionState MapState
        {
            get { return mapState; }
            set
            {
                mapState = value;

                switch (mapState)
                {
                    case MapPositionState.None:
                        VisualStateManager.GoToState(this, "None", true);
                        break;
                    case MapPositionState.Follow:
                        VisualStateManager.GoToState(this, "Follow", true);

                        // Set the center to the current position
                        if (currentPosition != null) MapMain.TrySetViewAsync(currentPosition.Point);
                        break;
                    case MapPositionState.FollowAndRotate:
                        VisualStateManager.GoToState(this, "FollowAndRotate", true);

                        // Set the center to the current position
                        if (currentPosition != null) MapMain.TrySetViewAsync(currentPosition.Point);
                        break;
                    default:
                        break;
                }

                OnPropertyChanged();
            }
        }

        private void MapMain_MapRightTapped(MapControl sender, MapRightTappedEventArgs args)
        {
            GeocodingLocation TapLocation = new GeocodingLocation(args.Location);

            MapPin newPin = new MapPin();
            newPin.DataContext = TapLocation;

            MapMain.Children.Add(newPin);
            MapControl.SetLocation(newPin, args.Location);
            MapControl.SetNormalizedAnchorPoint(newPin, new Point(0.5, 1));

            NavigationManager.Current.ShowLocation(TapLocation);
        }

        private void MapMain_MapTapped(Windows.UI.Xaml.Controls.Maps.MapControl sender, Windows.UI.Xaml.Controls.Maps.MapInputEventArgs args)
        {
        }

        private void MapMain_MapHolding(Windows.UI.Xaml.Controls.Maps.MapControl sender, Windows.UI.Xaml.Controls.Maps.MapInputEventArgs args)
        {
        }

        private void MapMain_ActualCameraChanged(MapControl sender, MapActualCameraChangedEventArgs args)
        {
            // Stop following the user if he changed the view
            if (MapState != MapPositionState.None && args.ChangeReason == MapCameraChangeReason.UserInteraction &&
                Math.Abs(args.Camera.Location.Position.Latitude - CurrentPosition.Point.Position.Latitude) > 0.0000001 &&
                Math.Abs(args.Camera.Location.Position.Longitude - CurrentPosition.Point.Position.Longitude) > 0.0000001)
                MapState = MapPositionState.None;

            // Stop rotating the map if the user rotates
            if (MapState == MapPositionState.FollowAndRotate && args.ChangeReason == MapCameraChangeReason.UserInteraction &&
                Math.Abs(args.Camera.Heading - CurrentHeading.HeadingTrueNorth.Value) > 0.0000001)
                MapState = MapPositionState.None;
        }

        #endregion

        #region Geolocation


        // Proides access to location data
        private Geolocator _geolocator = null;


        private MapGeolocationPin GeolocationPin;
        private MapPolygon AccuracyCircle;

        private ThreadPoolTimer PositionSaveTimer;

        private Geocoordinate currentPosition;

        public Geocoordinate CurrentPosition
        {
            get { return currentPosition; }
            set
            {
                currentPosition = value;

                // Cancel the Timer if it exists
                if (PositionSaveTimer != null) PositionSaveTimer.Cancel();


                if (currentPosition == null)
                {
                    GeolocationPin.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    AccuracyCircle.Visible = false;
                }
                else
                {
                    // Save the position after 10 seconds of no change
                    PositionSaveTimer = ThreadPoolTimer.CreateTimer(
                    (source) =>
                    {
                        // 
                        // Update the UI thread by using the UI core dispatcher.
                        // 
                        Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            SettingsManager.Current.LastGeoPosition = currentPosition.Point.ToPoint();
                        });
                    }, TimeSpan.FromSeconds(10));


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
        

        /// <summary>
        /// Start the Geolocator and the Compass
        /// </summary>
        /// <returns></returns>
        private async Task StartTracking()
        {
            // Compass
            _compass = Compass.GetDefault();
            if (_compass != null)
            {
                // Select a report interval that is both suitable for the purposes of the app and supported by the sensor.
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
                    

                    await ShowStatus("Suche aktuelle Position...");


                    break;

                case GeolocationAccessStatus.Denied:
                    await ShowStatus("Zugriff auf Position verweigert", StatusType.Error, 5, 0);
                    break;

                case GeolocationAccessStatus.Unspecified:
                    await ShowStatus("Fehler bei Ortung", StatusType.Error, 5, 0);
                    break;
            }
        }

        /// <summary>
        /// Stop the Geolocator and the Compass
        /// </summary>
        public void StopTracking()
        {
            if (_compass != null)
            {
                // Stop Compass
                _compass.ReadingChanged -= new TypedEventHandler<Compass, CompassReadingChangedEventArgs>(ReadingChanged);

                // Restore the default report interval to release resources while the sensor is not in use
                _compass.ReportInterval = 0;
            }


            // Stop location
            _geolocator.PositionChanged -= OnPositionChanged;
            _geolocator.StatusChanged -= OnStatusChanged;
            _geolocator = null;

            // Clear status
            ShowStatus("");
        }

        private enum StatusType
        {
            Info,
            Error
        }

        private ThreadPoolTimer StatusDelayTimer;

        /// <summary>
        /// Show a status in the UI (StatusBar on Mobile)
        /// </summary>
        /// <param name="Text">The text for the status. An empty string hides the Status.</param>
        /// <param name="Type">A StatusType</param>
        /// <param name="Duration">The duration of the Status in seconds (0 means forever)</param>
        /// <param name="Progress">The Progress Value (0-100), null means pending/indeterminate</param>
        /// <returns></returns>
        private async Task ShowStatus(string Text = "", StatusType Type = StatusType.Info, int Duration = 0, double? Progress = null)
        {
            // Cancel the Timer if it exists
            if (StatusDelayTimer != null) StatusDelayTimer.Cancel();


            // Show the Staus on the StatusBar on Mobile
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
            {
                StatusBar statusBar = StatusBar.GetForCurrentView();

                if (Text == "")
                {
                    // Hide the ProgressIndicator on the StatusBar
                    await statusBar.ProgressIndicator.HideAsync();
                    statusBar.ProgressIndicator.Text = "";

                    // Reset the BackgroundColor to transparent (for example if it was an error)
                    statusBar.BackgroundColor = Colors.Transparent;
                }
                else
                {
                    // Set the Text
                    statusBar.ProgressIndicator.Text = Text;

                    // Set the Progress
                    statusBar.ProgressIndicator.ProgressValue = Progress;

                    // Show the Type
                    if (Type == StatusType.Error) statusBar.BackgroundColor = Colors.Red;
                    else statusBar.BackgroundColor = Colors.Transparent;

                    // Show the ProgressIndicator
                    await statusBar.ProgressIndicator.ShowAsync();
                }
            }
            else // Show the Status in the StatusPanel
            {
                if (Text == "")
                {
                    // Just hide the Panel
                    StatusPanel.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
                else
                {
                    // Set the Text
                    StatusText.Text = Text;

                    // Set the ProgressBar
                    if (Progress == null) StatusProgress.IsIndeterminate = true;
                    else
                    {
                        StatusProgress.IsIndeterminate = false;
                        StatusProgress.Value = (double)Progress;
                    }
                    
                    // Show the Type
                    if (Type == StatusType.Error)
                    {
                        StatusProgress.ShowError = true;
                    }
                    else
                    {
                        StatusProgress.ShowError = false;
                    }

                    // And finally show the Panel itself
                    StatusPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }

            // If there's a Duration, start a Timer
            if (Text != "" && Duration > 0)
            {
                StatusDelayTimer = ThreadPoolTimer.CreateTimer(
                    (source) =>
                    {
                    // 
                    // Update the UI thread by using the UI core dispatcher.
                    // 
                    Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
                        {
                            ShowStatus();
                        });

                    }, TimeSpan.FromSeconds(Duration));
            }
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
                    ShowStatus("No Data", StatusType.Error, 5, 0);
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
                        ShowStatus("Geolocation is ready", StatusType.Info, 5, 0);
                        break;

                    case PositionStatus.Initializing:
                        // Location platform is attempting to acquire a fix. 
                        ShowStatus("Initializing");
                        break;

                    case PositionStatus.NoData:
                        // Location platform could not obtain location data.
                        ShowStatus("No data", StatusType.Error, 5, 0);
                        break;

                    case PositionStatus.Disabled:
                        // The permission to access location data is denied by the user or other policies.
                        ShowStatus("Disabled", StatusType.Error, 5, 0);

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
                        ShowStatus("Not available", StatusType.Error, 5, 0);
                        break;

                    default:
                        ShowStatus("Unknown", StatusType.Error, 5, 0);
                        GeolocationPin.Status = PositionStatus.NoData;
                        break;
                }
            });
        }


        #endregion

        #region Compass

        private Compass _compass;
        private uint _desiredReportInterval;
        CalibrationBar calibrationBar = new CalibrationBar();


        private CompassReading currentHeading;

        public CompassReading CurrentHeading
        {
            get { return currentHeading; }
            set
            {
                currentHeading = value;
                OnPropertyChanged();
            }
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

                CurrentHeading = e.Reading;

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


        #endregion

        #region Page size

        public static double SheetHalfFactor = 1.3;
        public static double HiddenSheetPeekSize = 80;
        
        private double pageWidth;

        public double PageWidth
        {
            get { return pageWidth; }
            set
            {
                pageWidth = value;
                OnPropertyChanged();
            }
        }

        private double pageHeight;

        public double PageHeight
        {
            get { return pageHeight; }
            set
            {
                pageHeight = value;
                OnPropertyChanged();
            }
        }

        private double sheetsFrameHeight;

        public double SheetsFrameHeight
        {
            get { return sheetsFrameHeight; }
            private set
            {
                sheetsFrameHeight = value;
                OnPropertyChanged();
            }
        }
        
        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            PageWidth = e.NewSize.Width;
            PageHeight = e.NewSize.Height;

            SetSnapPoints();

            // Handle the StatusBar on Mobile
            if (statusBar != null && e.NewSize.Height - BottomBar.ActualHeight - statusBar.OccludedRect.Height > 0)
            {
                SheetsFrameHeight = e.NewSize.Height - BottomBar.ActualHeight - statusBar.OccludedRect.Height;
            }
            else
            {
                SheetsFrameHeight = e.NewSize.Height - BottomBar.ActualHeight;
            }
        }

        private void SetSnapPoints()
        {
            List<float> NewList = new List<float>();
            NewList.Add((float)(PageHeight + (SheetsFrame.CurrentSourcePageType == typeof(LibrarySheet) ? 0 : HiddenSheetPeekSize)));
            NewList.Add((float)(PageHeight + (PageHeight - BottomBar.Height) / SheetHalfFactor));
            NewList.Add((float)(2 * PageHeight - BottomBar.Height));
            
            stackPanel.SnapPoints = NewList;

        }

        /// <summary>
        /// Handle Orientation Changes on Mobile
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RootPage_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (statusBar == null) return;

            // Turned to Landscape
            if (e.PreviousSize.Width <= e.PreviousSize.Height && e.NewSize.Width > e.NewSize.Height)
            {
                statusBar.HideAsync();

                StatusBarBackground.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            }
            // Turned to Portait
            else if (e.PreviousSize.Width >= e.PreviousSize.Height && e.NewSize.Width < e.NewSize.Height)
            {
                statusBar.ShowAsync();

                StatusBarBackground.Margin = new Thickness(0, -Window.Current.Bounds.Height, 0, Window.Current.Bounds.Height * 2 - BottomBar.Height - 2 * statusBar.OccludedRect.Height);
                StatusBarBackground.Height = Window.Current.Bounds.Height;
                StatusBarBackground.Visibility = Windows.UI.Xaml.Visibility.Visible;
            }
        }

        #endregion

        #region Map interaction buttons

        private void MapStylesButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
        }

        private void GeolocationButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            switch (MapState)
            {
                case MapPositionState.None:
                    MapState = MapPositionState.Follow;
                    break;
                case MapPositionState.Follow:
                    if (_compass != null) MapState = MapPositionState.FollowAndRotate;
                    break;
                case MapPositionState.FollowAndRotate:
                    MapState = MapPositionState.Follow;
                    break;
                default:
                    break;
            }
        }

        #endregion

    }

    public enum MapPositionState
    {
        None,
        Follow,
        FollowAndRotate
    }

}
