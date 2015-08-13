using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;


namespace TrolleyTracker.Controllers.WebAPI
{

    public class LocationController : ApiController
    {
        private TrolleyTrackerEntities db = new TrolleyTrackerEntities();



        // GET: api/Trolleys/5/Location
        public Trolley Get(int id)
        {
            if (!ModelState.IsValid)
            {
                return null;
            }

            var trolley = (from Trolley t in db.Trolleys
                           where t.Number == id
                           select t).FirstOrDefault<Trolley>();
            //if (trolley == null)
            //{
            //    return null;
            //}

            return trolley;
        }

        //         public IHttpActionResult PostLocation(int id, [FromBody] postString)


        // POST: api/Trolleys/1/Location
        /// <summary>
        /// Update Trolley location
        /// </summary>
        /// <param name="value"></param>
        [Authorize(Roles = "Vehicles")]
        public IHttpActionResult PostLocation(int id, LocationUpdate locationUpdate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var trolley = (from Trolley t in db.Trolleys
                           where t.Number == id
                           select t).FirstOrDefault<Trolley>();
            if (trolley == null)
            {
                return NotFound();
            }

            if ((locationUpdate.Lat < -90.0) || (locationUpdate.Lat > 90)) return BadRequest("Invalid latitude");
            if ((locationUpdate.Lon < -180.0) || (locationUpdate.Lon > 180)) return BadRequest("Invalid longitude");

            trolley.CurrentLat = locationUpdate.Lat;
            trolley.CurrentLon = locationUpdate.Lon;
            db.SaveChanges();
            TrolleyCache.UpdateTrolley(trolley);

            return CreatedAtRoute("DefaultApi", new { id = trolley.ID }, trolley);

        }

    }
}
