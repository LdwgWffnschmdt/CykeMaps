using CykeMaps.Core.Location;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace CykeMaps.Core.Route.RouteRequest
{
    /// <summary>
    /// This handles a basic web (REST API) based route request.
    /// To use it, just supply it with a Base URI and override the GetQueryParameters function and the ParseResults function.
    /// </summary>
    public abstract class BasicRouteRequest : IRouteRequest
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Fields

        private IList<ILocation> waypoints;

        public IList<ILocation> Waypoints
        {
            get { return waypoints; }
            set
            {
                waypoints = value;
                OnPropertyChanged();
            }
        }

        public bool Sendable
        {
            get
            {
                // Only try to send the Request if there are at least a Start and a Destination
                if (Waypoints.Count() >= 2 && Waypoints.Where(loc => loc.Location != null).Count() >= 2) return true;
                else return false;
            }
        }


        #endregion

        private string BaseURI;

        #region Constructor

        public BasicRouteRequest(string BaseURI)
        {
            this.BaseURI = BaseURI;
        }

        #endregion

        #region Functions

        /// <summary>
        /// Call this function to fire the Api call and get the result parsed as an IRoute object.
        /// </summary>
        /// <returns></returns>
        public async Task<IRoute> GetRoute()
        {
            if (!Sendable) throw new ArgumentOutOfRangeException("Sendable", "This Request is not yet ready to be sent");
            
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            var httpClient = new HttpClient();

            UriBuilder builder = new UriBuilder(BaseURI);
            builder.Port = -1;
            builder.Query = ToQueryString(GetQueryParameters());
            


            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(builder.Uri, cts.Token);
                
                // Only go on if it was successful
                //response.EnsureSuccessStatusCode(); // TODO: Supply the user with info about the error

                // Get the content of the response as string
                string responseContent = await response.Content.ReadAsStringAsync();

                // Return the parsed content as a Route object
                return ParseResult(responseContent);
            }
            catch (TaskCanceledException ex)
            {
                throw ex;
            }
            catch (HttpRequestException ex)
            {
                throw ex;
            }

        }

        public abstract NameValueCollection GetQueryParameters();

        public abstract BasicRoute ParseResult(string response);

        #endregion

        private string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", WebUtility.UrlEncode(key), WebUtility.UrlEncode(value)))
                .ToArray();
            return string.Join("&", array);
        }
    }
}
