using CykeMaps.Core.Location;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Devices.Geolocation;

namespace CykeMaps.Core.Route.RouteRequest
{
    public class KomootRouteRequest : BasicRouteRequest
    {
        #region Enums

        /// <summary>
        /// Sportart
        /// </summary>
        public enum Sportart
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

        public enum ResponseFormat
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
        public int Constitution { get; set; }

        public Sportart Sport { get; set; }

        /// <summary>
        /// The return format. Must be of type ResponseFormat
        /// </summary>
        public ResponseFormat Format { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new KomootRouteRequest with default values for Constitution and Sport
        /// </summary>
        public KomootRouteRequest() : base("https://www.komoot.de/api/routing/route")
        {
            // Set default values
            Constitution = 3;
            Sport = Sportart.mtb_easy;
            Format = ResponseFormat.coordinate_array; // Must be coordinate_array for parsing
            Waypoints = new List<ILocation>();
        }

        /// <summary>
        /// Create a new KomootRouteRequest with a predefined destination
        /// </summary>
        /// <param name="destination">Where to navigate to</param>
        public KomootRouteRequest(ILocation destination) : this()
        {
            Waypoints = new List<ILocation>()
            {
                new BasicLocation()
                {
                    Location = new Geopoint(new BasicGeoposition()
                    {
                        Latitude =  SettingsManager.Current.LastGeoPosition.X,
                        Longitude =  SettingsManager.Current.LastGeoPosition.Y
                    })
                },
                destination
            };
        }

        /// <summary>
        /// Create a new KomootRouteRequest with a predefined destination and a starting point
        /// </summary>
        /// <param name="destination">Where to navigate to</param>
        /// <param name="start">Where to start the route</param>
        public KomootRouteRequest(ILocation destination, ILocation start) : this()
        {
            Waypoints = new List<ILocation>()
            {
                start,
                destination
            };
        }

        #endregion

        public override NameValueCollection GetQueryParameters()
        {
            NameValueCollection ParameterCollection = new NameValueCollection();

            // Build the path (format: lat,lng lat,lng lat,lng ...)
            StringBuilder PathBuilder = new StringBuilder();
            foreach (ILocation Waypoint in Waypoints)
            {
                if (PathBuilder.Length > 0) PathBuilder.Append(" ");
                PathBuilder.Append(Waypoint.Location.Position.Latitude.ToString(CultureInfo.InvariantCulture) + "," + Waypoint.Location.Position.Longitude.ToString(CultureInfo.InvariantCulture));
            }
            ParameterCollection.Add("path", PathBuilder.ToString());

            if (Constitution >= 1 && Constitution <= 5) ParameterCollection.Add("constitution", Constitution.ToString(CultureInfo.InvariantCulture));

            ParameterCollection.Add("sport", Sport.ToString());
            ParameterCollection.Add("format", Format.ToString());

            return ParameterCollection;
        }

        private const string trackKey = "track";
        private const string distanceKey = "distance";
        private const string durationKey = "duration";
        private const string uphillKey = "uphill";
        private const string downhillKey = "downhill";
        private const string sportKey = "sport";
        private const string constitutionKey = "constitution";
        private const string waytypesKey = "waytypes";
        private const string surfacesKey = "surfaces";
        private const string directionsKey = "directions";
        private const string summaryKey = "summary";
        private const string difficultyKey = "difficulty";
        private const string wayScoreKey = "wayScore";
        private const string nameKey = "name";
        private const string startPointKey = "startPoint";

        public override BasicRoute ParseResult(string response)
        {
            // Parse the Result (it is JSON)
            JsonObject jsonObject = JsonObject.Parse(response);

            // Create a new Route
            BasicRoute Route = new BasicRoute();


            IJsonValue nameJsonValue = jsonObject.GetNamedValue(nameKey);
            if (nameJsonValue.ValueType == JsonValueType.Null)
            {
                Route.Name = "New Route"; // TODO: Give it a name!
            }
            else
            {
                Route.Name = nameJsonValue.GetString();
            }



            IJsonValue distanceJsonValue = jsonObject.GetNamedValue(distanceKey);
            if (distanceJsonValue.ValueType == JsonValueType.Null)
            {
                Route.Distance = null; // TODO: If there is no distance, calculate it ourselves!
            }
            else
            {
                Route.Distance = distanceJsonValue.GetNumber();
            }
            


            IJsonValue upHillJsonValue = jsonObject.GetNamedValue(uphillKey);
            if (upHillJsonValue.ValueType == JsonValueType.Null)
            {
                Route.Uphill = null; // Null means no info in this case
            }
            else
            {
                Route.Uphill = upHillJsonValue.GetNumber();
            }



            IJsonValue downHillJsonValue = jsonObject.GetNamedValue(downhillKey);
            if (downHillJsonValue.ValueType == JsonValueType.Null)
            {
                Route.Downhill = null; // Null means no info in this case
            }
            else
            {
                Route.Downhill = downHillJsonValue.GetNumber();
            }


            JsonObject startPointJsonObject = jsonObject.GetNamedObject(startPointKey);
            Route.StartPoint = new BasicLocation()
            {
                Location = new Geopoint(new BasicGeoposition()
                {
                    Latitude = startPointJsonObject.GetNamedNumber("lat", Waypoints.ElementAt(0).Location.Position.Latitude), // Just use the info we already have if there is none
                    Longitude = startPointJsonObject.GetNamedNumber("lng", Waypoints.ElementAt(0).Location.Position.Longitude),
                    Altitude = startPointJsonObject.GetNamedNumber("alt", Waypoints.ElementAt(0).Location.Position.Altitude)
                })
            };


            List<BasicGeoposition> Track = new List<BasicGeoposition>();
            foreach (IJsonValue jsonValue in jsonObject.GetNamedArray(trackKey, new JsonArray()))
            {
                if (jsonValue.ValueType == JsonValueType.Object)
                {
                    JsonObject jsonValueAsObject = jsonValue.GetObject();
                    Track.Add(new BasicGeoposition()
                    {
                        Latitude = jsonValueAsObject.GetNamedNumber("lat"),
                        Longitude = jsonValueAsObject.GetNamedNumber("lng"),
                        Altitude = jsonValueAsObject.GetNamedNumber("alt")
                    });
                }
            }
            Route.Track = Track;



            return Route;
        }
    }
}
