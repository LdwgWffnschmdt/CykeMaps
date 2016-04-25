using CykeMaps.Core.Actions;
using CykeMaps.Core.Route;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using System;

namespace CykeMaps.Core.Route
{
    public class BasicRoute : IRoute
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        

        public BasicRoute()
        {
            actions = new List<IAction>()
                {
                    new AddFavoriteAction(),
                    new ShowOnMapAction()
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

        private GeoboundingBox boundingBox;

        public GeoboundingBox BoundingBox
        {
            get { return boundingBox; }
            set
            {
                boundingBox = value;
                OnPropertyChanged();
            }
        }

        private string symbol = "Arrow";

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

        public IEnumerable<BasicGeoposition> Positions
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public BasicGeoposition StartPoint
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public double Distance
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public double Downhill
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public double Uphill
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }
    }
}