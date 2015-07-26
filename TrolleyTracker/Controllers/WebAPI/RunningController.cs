using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;

namespace TrolleyTracker.Controllers.WebAPI
{
    public class RunningController : ApiController
    {
        // Mapped as - GET: api/Trolleys/Running
        public List<RunningTrolley> Get()
        {
            return TrolleyCache.GetRunningTrolleys();
        }

    }
}
