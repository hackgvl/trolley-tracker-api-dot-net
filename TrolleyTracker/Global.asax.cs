using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;
using NLog;

namespace TrolleyTracker
{
    public class MvcApplication : System.Web.HttpApplication
    {

        ///// <summary>
        ///// To debug the WebApi routing
        ///// </summary>
        //public override void Init()
        //{
        //    base.Init();
        //    this.AcquireRequestState += showRouteValues;
        //}

        ///// <summary>
        ///// To help debug WebApi routing only
        ///// </summary>
        ///// <param name="sender"></param>
        ///// <param name="e"></param>
        //protected void showRouteValues(object sender, EventArgs e)
        //{
        //    var context = HttpContext.Current;
        //    if (context == null)
        //        return;
        //    var routeData = RouteTable.Routes.GetRouteData(new HttpContextWrapper(context));
        //}

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Application_EndRequest()
        {
            var context = new HttpContextWrapper(this.Context);

            // If we're a web API client and forms authentication caused a 302, 
            // then we actually need to do a 401
            if (context.Response.StatusCode == 302 && context.Request.Path.StartsWith("/api/v") )
            {
                context.Response.Clear();
                context.Response.StatusCode = 401;
            }

            // Remove cookies from WebAPI response - they're not needed or used by our clients
            if (context.Request.Path.StartsWith("/api/v"))
            {
                Context.Response.Cookies.Clear();
            }
        }


        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            Exception lastException = Server.GetLastError();
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            var message = String.Format($"Unhandled Exception happened: {lastException.GetType()}; with message: {lastException.Message}");
            logger.Error(lastException, message);

            Server.ClearError(); //clear the error so we can continue onwards
            Response.Redirect("~/Error"); //direct user to error page

        }

        void Page_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs
            Exception lastException = Server.GetLastError();
            NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
            var message = String.Format($"Unhandled Exception happened: {lastException.GetType()}; with message: {lastException.Message}");
            logger.Error(lastException, message);

            Server.ClearError(); //clear the error so we can continue onwards
            Response.Redirect("~/Error"); //direct user to error page

        }
    }
}
