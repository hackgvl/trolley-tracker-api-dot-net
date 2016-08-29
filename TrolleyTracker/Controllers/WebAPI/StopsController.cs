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
        private TrolleyTrackerContext db = new TrolleyTrackerContext();

        // GET: api/Stops
        public List<StopSummary> GetStops()
        {
            //var stops = db.Stops;
            //var stopList = new List<StopSummary>();
            //foreach(var stop in stops)
            //{
            //    stopList.Add(new StopSummary(stop));
            //}
            //return stopList;
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

        //// PUT: api/Stops/5
        //[ResponseType(typeof(void))]
        //public IHttpActionResult PutStop(int id, Stop stop)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    if (id != stop.ID)
        //    {
        //        return BadRequest();
        //    }

        //    db.Entry(stop).State = EntityState.Modified;

        //    try
        //    {
        //        db.SaveChanges();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!StopExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return StatusCode(HttpStatusCode.NoContent);
        //}

        //// POST: api/Stops
        //[ResponseType(typeof(Stop))]
        //public IHttpActionResult PostStop(Stop stop)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    db.Stops.Add(stop);
        //    db.SaveChanges();

        //    return CreatedAtRoute("DefaultApi", new { id = stop.ID }, stop);
        //}

        //// DELETE: api/Stops/5
        //[ResponseType(typeof(Stop))]
        //public IHttpActionResult DeleteStop(int id)
        //{
        //    Stop stop = db.Stops.Find(id);
        //    if (stop == null)
        //    {
        //        return NotFound();
        //    }

        //    db.Stops.Remove(stop);
        //    db.SaveChanges();

        //    return Ok(stop);
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StopExists(int id)
        {
            return db.Stops.Count(e => e.ID == id) > 0;
        }
    }
}