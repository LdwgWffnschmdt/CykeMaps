using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;

namespace CykeMaps.UI.Navigation
{
    public enum Visibility
    {
        Undefined = 0,
        Hidden = 1,
        Half = 2,
        Full = 3
    }

    public class State : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private Type sheet;
        private bool hasSheet = false;

        private Visibility sheetVisibility;
        private bool hasSheetVisibility = false;

        private bool isModal = false; // Defaults to false

        private Action onBackAction;
        private bool hasOnBackAction;

        private IList<MapElement> mapElements;
        private bool hasMapElements = false;

        private Geopoint mapCenter;
        private bool hasMapCenter = false;

        private double mapZoomLevel;
        private bool hasMapZoomLevel = false;

        private double mapPitch;
        private bool hasMapPitch = false;

        private double mapHeading;
        private bool hasMapHeading = false;


        public Type Sheet
        {
            get { return sheet; }
            set
            {
                sheet = value;
                hasSheet = true;
                OnPropertyChanged();
            }
        }

        public bool HasSheet { get { return hasSheet; } }

        /// <summary>
        /// Basically, if the sheet is scrolled up or not
        /// </summary>
        public Visibility SheetVisibility
        {
            get { return sheetVisibility; }
            set
            {
                sheetVisibility = value;
                hasSheetVisibility = true;
                OnPropertyChanged();
            }
        }

        public bool HasSheetVisibility { get { return hasSheetVisibility; } }

        /// <summary>
        /// The Action is called when going back
        /// </summary>
        public Action OnBackAction
        {
            get { return onBackAction; }
            set
            {
                onBackAction = value;
                hasOnBackAction = true;
                OnPropertyChanged();
            }
        }

        public bool HasOnBackAction { get { return hasOnBackAction; } }

        /// <summary>
        /// Specifies if this State is just a modal one. Tru means you cannot go back to this State.
        /// </summary>
        public bool IsModal
        {
            get
            {
                return isModal;
            }
            set
            {
                isModal = value;
            }
        }

        // Map

        public IList<MapElement> MapElements
        {
            get { return mapElements; }
            set
            {
                mapElements = value;
                hasMapElements = true;
                OnPropertyChanged();
            }
        }

        public bool HasMapElements { get { return hasMapElements; } }

        public Geopoint MapCenter
        {
            get { return mapCenter; }
            set
            {
                mapCenter = value;
                hasMapCenter = true;
                OnPropertyChanged();
            }
        }

        public bool HasMapCenter { get { return hasMapCenter; } }

        public double MapZoomLevel
        {
            get { return mapZoomLevel; }
            set
            {
                mapZoomLevel = value;
                hasMapZoomLevel = true;
                OnPropertyChanged();
            }
        }

        public bool HasMapZoomLevel { get { return hasMapZoomLevel; } }

        public double MapPitch
        {
            get { return mapPitch; }
            set
            {
                mapPitch = value;
                hasMapPitch = true;
                OnPropertyChanged();
            }
        }

        public bool HasMapPitch { get { return hasMapPitch; } }

        public double MapHeading
        {
            get { return mapHeading; }
            set
            {
                mapHeading = value;
                hasMapHeading= true;
                OnPropertyChanged();
            }
        }

        public bool HasMapHeading { get { return hasMapHeading; } }


        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine("------------------------STATE------------------------");
            builder.AppendLine("Sheet: " + ((Sheet == null) ? "null" : Sheet.ToString()));
            builder.AppendLine("SheetVisibility: " + SheetVisibility.ToString());
            builder.AppendLine("MapCenter: " + ((MapCenter == null) ? "null" : (MapCenter.Position.Latitude.ToString() + ", " + MapCenter.Position.Longitude.ToString())));
            builder.AppendLine("MapZoomLevel: " + MapZoomLevel.ToString());
            builder.AppendLine("MapPitch: " + MapPitch.ToString());
            builder.AppendLine("MapHeading: " + MapHeading.ToString());
            builder.AppendLine("HasSheet: " + HasSheet.ToString());
            builder.AppendLine("HasSheetVisibility: " + HasSheetVisibility.ToString());
            builder.AppendLine("HasMapCenter: " + HasMapCenter.ToString());
            builder.AppendLine("HasMapZoomLevel: " + HasMapZoomLevel.ToString());
            builder.AppendLine("HasMapPitch: " + HasMapPitch.ToString());
            builder.AppendLine("HasMapHeading: " + HasMapHeading.ToString());
            builder.AppendLine("-----------------------------------------------------");
            return builder.ToString();
        }
    }
}
