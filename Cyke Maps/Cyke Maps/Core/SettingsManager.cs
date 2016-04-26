using CykeMaps.Core.Location;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Maps;

namespace CykeMaps.Core
{
    public sealed partial class SettingsManager : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }


        private ApplicationDataContainer RoamingSettings;
        private ApplicationDataContainer LocalSettings;
        private MapControl MapMain;

        /// <summary>
        /// This holds the instance to the Only SettingsManager in this app.
        /// </summary>
        public static SettingsManager Current { get; protected set; }

        public SettingsManager(ref MapControl MapMain)
        {
            // Check is the instance doesnt already exist.
            if (Current != null)
            {
                //if there is an instance in the app already present then simply throw an error.
                throw new Exception("Only one settings manager can exist in a App.");
            }

            // Setting the instance to the static instance field.
            Current = this;

            this.MapMain = MapMain;

            ApplicationData.Current.DataChanged += new TypedEventHandler<ApplicationData, object>(DataChangeHandler);

            // Roaming Settings
            RoamingSettings = ApplicationData.Current.RoamingSettings;

            RoamingSettings.CreateContainer("Map", ApplicationDataCreateDisposition.Always);
            RoamingSettings.CreateContainer("Appearance", ApplicationDataCreateDisposition.Always);
            

            // Local Settings
            LocalSettings = ApplicationData.Current.LocalSettings;

            LocalSettings.CreateContainer("Location", ApplicationDataCreateDisposition.Always);
        }


        void DataChangeHandler(ApplicationData appData, object o)
        {

        }

        /// <summary>
        /// Gets called on app suspend and should save all settings that are not saved yet
        /// </summary>
        public void SaveSettings()
        {
            LastMapCenter = MapMain.Center.ToPoint();
            LastMapZoom = MapMain.ZoomLevel;
            LastMapHeading = MapMain.Heading;
            LastMapPitch = MapMain.Pitch;
        }


        #region General Settings

        public bool UseLocation
        {
            get
            {
                return (bool)(RoamingSettings.Values.ContainsKey("UseLocation") ? RoamingSettings.Values["UseLocation"] : true);
            }
            set
            {
                RoamingSettings.Values["UseLocation"] = value;
                OnPropertyChanged();
            }
        }


        // Only save this locally
        public Point LastGeoPosition
        {
            get
            {
                return (Point)(LocalSettings.Containers["Location"].Values.ContainsKey("LastGeoPosition") ? LocalSettings.Containers["Location"].Values["LastGeoPosition"] : new Point());
            }
            set
            {
                LocalSettings.Containers["Location"].Values["LastGeoPosition"] = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Appearance


        public ElementTheme AppTheme
        {
            get
            {
                return (ElementTheme)(RoamingSettings.Containers["Appearance"].Values.ContainsKey("AppTheme") ? RoamingSettings.Containers["Appearance"].Values["AppTheme"] : ElementTheme.Default);
            }
            set
            {
                RoamingSettings.Containers["Appearance"].Values["AppTheme"] = (int)value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Map Settings


        public bool ShowZoomControl
        {
            get
            {
                return (bool)(RoamingSettings.Containers["Map"].Values.ContainsKey("ShowZoomControl") ? RoamingSettings.Containers["Map"].Values["ShowZoomControl"] : true);
            }
            set
            {
                RoamingSettings.Containers["Map"].Values["ShowZoomControl"] = value;
                OnPropertyChanged();
            }
        }
        
        public bool ShowTiltControl
        {
            get
            {
                return (bool)(RoamingSettings.Containers["Map"].Values.ContainsKey("ShowTiltControl") ? RoamingSettings.Containers["Map"].Values["ShowTiltControl"] : true);
            }
            set
            {
                RoamingSettings.Containers["Map"].Values["ShowTiltControl"] = value;
                OnPropertyChanged();
            }
        }

        public bool ShowRotationControl
        {
            get
            {
                return (bool)(RoamingSettings.Containers["Map"].Values.ContainsKey("ShowRotationControl") ? RoamingSettings.Containers["Map"].Values["ShowRotationControl"] : true);
            }
            set
            {
                RoamingSettings.Containers["Map"].Values["ShowRotationControl"] = value;
                OnPropertyChanged();
            }
        }

        public Point LastMapCenter
        {
            get
            {
                return (Point)(RoamingSettings.Containers["Map"].Values.ContainsKey("LastMapCenter") ? RoamingSettings.Containers["Map"].Values["LastMapCenter"] : new Point(52.5, 13.38));
            }
            set
            {
                RoamingSettings.Containers["Map"].Values["LastMapCenter"] = value;
                OnPropertyChanged();
            }
        }

        public double LastMapZoom
        {
            get
            {
                return (double)(RoamingSettings.Containers["Map"].Values.ContainsKey("LastMapZoom") ? RoamingSettings.Containers["Map"].Values["LastMapZoom"] : 13.0);
            }
            set
            {
                RoamingSettings.Containers["Map"].Values["LastMapZoom"] = value;
                OnPropertyChanged();
            }
        }

        public double LastMapHeading
        {
            get
            {
                return (double)(RoamingSettings.Containers["Map"].Values.ContainsKey("LastMapHeading") ? RoamingSettings.Containers["Map"].Values["LastMapHeading"] : 0.0);
            }
            set
            {
                RoamingSettings.Containers["Map"].Values["LastMapHeading"] = value;
                OnPropertyChanged();
            }
        }

        public double LastMapPitch
        {
            get
            {
                return (double)(RoamingSettings.Containers["Map"].Values.ContainsKey("LastMapPitch") ? RoamingSettings.Containers["Map"].Values["LastMapPitch"] : 0.0);
            }
            set
            {
                RoamingSettings.Containers["Map"].Values["LastMapPitch"] = value;
                OnPropertyChanged();
            }
        }
        #endregion
    }
}
