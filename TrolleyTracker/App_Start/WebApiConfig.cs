using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace TrolleyTracker
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {

            // Web API configuration and services

            TrolleyTracker.Models.TrolleyCache.Initialize();
            TrolleyTracker.Models.StopArrivalTime.Initialize();
            Controllers.AppSettingsInterface.LoadAppSettings();

            // Web API routes
            config.MapHttpAttributeRoutes();

            // Matches general request with ID
            config.Routes.MapHttpRoute(
                name: "GeneralTrolley",
                routeTemplate: "api/v1/{controller}/{id}",
                defaults: new { },
                constraints: new { id = @"\d+" }
            );

            config.Routes.MapHttpRoute(
                name: "RunningTrolleys",
                routeTemplate: "api/v1/Trolleys/{controller}"
            );

            config.Routes.MapHttpRoute(
                name: "ActiveRoutes",
                routeTemplate: "api/v1/Routes/{controller}"
            );

            config.Routes.MapHttpRoute(
                name: "RegularStops",
                routeTemplate: "api/v1/Stops/{controller}"
            );

            config.Routes.MapHttpRoute(
                name: "TrolleyLocationDEPRECATED",
                routeTemplate: "api/v1/trolly/{id}/{controller}",
                defaults: new { },
                constraints: new { id = @"\d+" }
            );

            config.Routes.MapHttpRoute(
                name: "TrolleyLocation",
                routeTemplate: "api/v1/Trolleys/{id}/{controller}",
                defaults: new { },
                constraints: new { id = @"\d+" }
            );


            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/v1/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // Make default return type as JSON in browser.
            // XML is still supported when explicitly requested
            GlobalConfiguration.Configuration.Formatters.JsonFormatter.MediaTypeMappings
                .Add(new RequestHeaderMapping("Accept",
                              "text/html",
                              StringComparison.InvariantCultureIgnoreCase,
                              true,
                              "application/json"));
        }
    }
}
