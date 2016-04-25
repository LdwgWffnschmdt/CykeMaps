using CykeMaps.Core.Actions;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Devices.Geolocation;

namespace CykeMaps.Core.Location
{
    public interface ILocation : INotifyPropertyChanged
    {
        string Name { get; set; }

        string Address { get; set; }

        string Description { get; set; }

        Geopoint Location { get; set; }

        string Symbol { get; set; }

        List<IAction> Actions { get; }

        List<IAction> SecondaryActions { get; }
    }
}