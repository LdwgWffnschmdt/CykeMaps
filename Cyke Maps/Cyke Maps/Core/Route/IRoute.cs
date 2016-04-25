using CykeMaps.Core.Actions;
using System.Collections.Generic;
using System.ComponentModel;
using Windows.Devices.Geolocation;

namespace CykeMaps.Core.Route
{
    public interface IRoute : INotifyPropertyChanged
    {
        string Name { get; set; }
        
        string Description { get; set; }

        string Symbol { get; set; }

        GeoboundingBox BoundingBox { get; set; }

        IEnumerable<BasicGeoposition> Positions { get; set; }

        BasicGeoposition StartPoint { get; set; }

        double Distance { get; set; }

        double Downhill { get; set; }

        double Uphill { get; set; }



        List<IAction> Actions { get; }

        List<IAction> SecondaryActions { get; }
    }
}