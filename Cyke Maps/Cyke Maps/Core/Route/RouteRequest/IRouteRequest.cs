using CykeMaps.Core.Location;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace CykeMaps.Core.Route.RouteRequest
{
    public interface IRouteRequest : INotifyPropertyChanged
    {
        /// <summary>
        /// Start, Points in between and Finish
        /// </summary>
        IList<ILocation> Waypoints { get; set; }

        Task<IRoute> GetRoute();

        bool Sendable { get; }
    }
}
