using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TrolleyTracker.ViewModels;
using TrolleyTracker.Models;
using System.Data.Entity;

namespace TrolleyTracker.Controllers.WebAPI
{
    public class ActiveController : ApiController
    {

        // Mapped as - GET: api/Routes/Active
        public List<RouteSummary> Get()
        {
            return ActiveRoutes.GetActiveRoutes();
        }


    }
}
