using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading;

namespace GreenlinkTracker
{
    /// <summary>
    /// Interface to Syncromatics API used by Greenlink
    /// </summary>
    public class Syncromatics
    {
        private const string DefaultProductionAPIUrl = "https://api.syncromatics.com/portal";
        private const string APIKey = "d33d775c3a1c16a35ddbb3763a4187257c56661b10f1b8a17143d614d301bc08";   // Assigned only to this application
        //private const string APIKey = "8b6dfac1a48f44d680abf2d9706233cd34de2b47eae83945f6e80674699299ad";
        private string apiURL;
        private CancellationToken cancellationToken;

        // Single static client to avoid taking too many sockets with dynamic allocation
        private static readonly HttpClient httpClient = new HttpClient();

        public Syncromatics(CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            Initialize(DefaultProductionAPIUrl);
        }

        public Syncromatics(string apiURL, CancellationToken cancellationToken)
        {
            this.cancellationToken = cancellationToken;
            Initialize(apiURL);
        }

        private void Initialize(string apiURL)
        {
            this.apiURL = apiURL;

            httpClient.DefaultRequestHeaders.Add("User-Agent", "TrolleyTrackerServer");
            httpClient.DefaultRequestHeaders.Add("Api-Key", APIKey);

            httpClient.DefaultRequestHeaders
              .Accept
              .Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
        }


        public async Task<IList<Route>> GetRoutes()
        {
            var strRouteJSON = await GetRequest("routes");
            var routes = JsonConvert.DeserializeObject<IList<Syncromatics.Route>>(strRouteJSON);
            return routes;
        }

        /// <summary>
        /// Service type query: currently used to differentiate between Fixed Routes and Trolley
        /// </summary>
        /// <returns>Services and their associated routes</returns>
        public async Task<IList<Service>> GetServices()
        {
            var strServicesJSON = await GetRequest("services");

            var services = JsonConvert.DeserializeObject<IList<Syncromatics.Service>>(strServicesJSON);
            return services;
        }


        public async Task<IList<Vehicle>> GetVehiclesOnRoute(int routeID)
        {
            var strVehicleJSON = await GetRequest($"routes/{routeID}/vehicles");
            var vehicles = JsonConvert.DeserializeObject<IList<Syncromatics.Vehicle>>(strVehicleJSON);
            return vehicles;
        }


        public async Task<string> GetRequest(string request)
        {
            using (var result = await httpClient.GetAsync($"{apiURL}/{request}", cancellationToken))
            {
                string content = await result.Content.ReadAsStringAsync();
                if (!result.IsSuccessStatusCode)
                {
                    throw new SyncromaticsException($"HTTP Error {result.StatusCode} querying Syncromatics API for '{request}' returned '{content}'");
                }
                return content;
            }
        }


        public class Route
        {
            public int id { get; set; }
            public string color { get; set; }
            public string textColor { get; set; }
            public int displayOrder { get; set; }
            public string name { get; set; }
            public string shortName { get; set; }
            public string customerRouteId { get; set; }
            public string description { get; set; }
            public string url { get; set; }
            public string routeType { get; set; }

        }

        public class Service
        {

            public int id { get; set; }
            public string name { get; set; }
            public int displayOrder { get; set; }
            public IList<Route> routes { get; set; }

        }


        public class Vehicle
        {

            public int id { get; set; }
            public int capacity { get; set; }
            public double lat { get; set; }
            public double lon { get; set; }
            public string name { get; set; }
            public double passengerLoad { get; set; }
            // in UTC
            public DateTime lastUpdated { get; set; }
        }

        public class SyncromaticsException : Exception
        {
            public SyncromaticsException()
            {
            }

            public SyncromaticsException(string message)
                : base(message)
            {
            }

            public SyncromaticsException(string message, Exception inner)
                : base(message, inner)
            {
            }
        }


    }
}
