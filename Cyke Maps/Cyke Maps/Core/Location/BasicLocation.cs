using CykeMaps.Core.Location.Actions;
using System.Collections.Generic;
using Windows.Devices.Geolocation;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Foundation;

namespace CykeMaps.Core.Location
{
    public class BasicLocation : ILocation
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        

        public BasicLocation()
        {
            actions = new List<IAction>()
                {
                    new AddFavoriteAction(),
                    new ShowOnMapAction(),
                    new RouteToAction()
                };

            secondaryActions = new List<IAction>()
                {
                    new PinToStartAction(),
                    new ShareAction()
                };
        }

        private Point normalizedAnchorPoint = new Point() { X = 0.5, Y = 1 };
        public Point NormalizedAnchorPoint
        {
            get
            {
                return normalizedAnchorPoint;
            }
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


        private string address;

        public string Address
        {
            get { return address; }
            set
            {
                address = value;
                OnPropertyChanged();
            }
        }

        private string description;

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged();
            }
        }

        private Geopoint location;

        public Geopoint Location
        {
            get { return location; }
            set
            {
                location = value;
                OnPropertyChanged();
            }
        }

        private string symbol = "Target";

        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                OnPropertyChanged();
            }
        }

        private List<IAction> actions;

        public List<IAction> Actions
        {
            get
            {
                return actions;
            }
            set
            {
                actions = value;
                OnPropertyChanged();
            }
        }

        private List<IAction> secondaryActions;

        public List<IAction> SecondaryActions
        {
            get
            {
                return secondaryActions;
            }
            set
            {
                secondaryActions = value;
                OnPropertyChanged();
            }
        }
    }
}