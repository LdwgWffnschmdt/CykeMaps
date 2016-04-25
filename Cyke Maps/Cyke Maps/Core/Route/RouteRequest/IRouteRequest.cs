using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace CykeMaps.Core.Route.RouteRequest
{
    public interface IRouteRequest : INotifyPropertyChanged
    {
        /// <summary>
        /// Start, Points in between and Finish
        /// </summary>
        IEnumerable<BasicGeoposition> Waypoints { get; set; }

        Task<BasicRoute> GetRoute();

        string GetParameters();
        
    }
}
