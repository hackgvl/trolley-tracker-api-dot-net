using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;

namespace TrolleyTracker.Controllers.WebAPI
{
    public class StopsController : ApiController
    {
        // GET: api/Stops
        public List<StopSummary> GetStops()
        {
            return StopArrivalTime.StopSummaryListWithArrivalTimes;
        }

        // GET: api/Stops/5
        [ResponseType(typeof(StopSummary))]
        public IHttpActionResult GetStop(int id)
        {
            var stopSummary = StopArrivalTime.GetStopSummaryWithArrivalTimes(id);
            if (stopSummary == null)
            {
                return NotFound();
            }

            return Ok(stopSummary);
        }


        protected override void Dispose(bool disposing)
        {
            //if (disposing)
            //{
            //    db.Dispose();
            //}
            base.Dispose(disposing);
        }

        //private bool StopExists(int id)
        //{
        //    return db.Stops.Count(e => e.ID == id) > 0;
        //}
    }
}