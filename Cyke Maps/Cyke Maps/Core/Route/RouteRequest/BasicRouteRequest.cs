using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace CykeMaps.Core.Route.RouteRequest
{
    class BasicRouteRequest : IRouteRequest
    {
        public event PropertyChangedEventHandler PropertyChanged = delegate { };

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            // Raise the PropertyChanged event, passing the name of the property whose value has changed.
            this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #region Constructor

        public BasicRouteRequest()
        {

        }

        #endregion

        #region Fields
       
        private IEnumerable<BasicGeoposition> waypoints;

        public IEnumerable<BasicGeoposition> Waypoints
        {
            get { return waypoints; }
            set
            {
                waypoints = value;
                OnPropertyChanged();
            }
        }


        #endregion

        private string BaseURI;

        #region Functions

        public async Task<BasicRoute> GetRoute()
        {
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            var httpClient = new HttpClient();
            var resourceUri = new Uri("http://www.contoso.com");

            UriBuilder builder = new UriBuilder(BaseURI);
            builder.Port = -1;

            //var query = HttpUtility.ParseQueryString(builder.Query);
            

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(resourceUri, cts.Token);
                return ParseResult(response);
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

        private BasicRoute ParseResult(HttpResponseMessage response)
        {
            throw new NotImplementedException();
        }

        public string GetParameters()
        {
            return "";
        }

        #endregion
    }
}
