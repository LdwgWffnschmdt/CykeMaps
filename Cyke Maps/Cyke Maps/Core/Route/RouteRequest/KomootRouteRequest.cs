using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace CykeMaps.Core.Route.RouteRequest
{
    class KomootRouteRequest
    {
        #region Enums

        /// <summary>
        /// Sportart
        /// </summary>
        public enum Sport
        {
            /// <summary>
            /// Wandern
            /// </summary>
            hike,
            /// <summary>
            /// Bergtour
            /// </summary>
            mountaineering,
            /// <summary>
            /// Fahrrad
            /// </summary>
            touringbicycle,
            /// <summary>
            /// Fahrrad (mit Schotter)
            /// </summary>
            mtb_easy,
            /// <summary>
            /// Mountainbike
            /// </summary>
            mtb,
            /// <summary>
            /// Mountainbike (Alpin)
            /// </summary>
            mtb_advanced,
            /// <summary>
            /// Rennrad
            /// </summary>
            racebike,
            /// <summary>
            /// Laufen (BETA)
            /// </summary>
            jogging
        }

        public enum Format
        {
            minimal,
            simple,
            encoded_polyline,
            coordinate_array
        }

        #endregion

        #region Fields

        /// <summary>
        /// 1: Untrainiert
        /// 2: Durchschnittlich
        /// 3: Gut in Form
        /// 4: Sehr sportlich
        /// 5: Profi
        /// </summary>
        public int constitution { get; set; }

        public Sport sport { get; set; }

        public Format format { get; set; }

        #endregion
    }
}
