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

namespace TrolleyTracker.Controllers.WebAPI
{
    public class TrolleysController : ApiController
    {
        private TrolleyTrackerEntities db = new TrolleyTrackerEntities();

        // GET: api/Trolleys
        public IQueryable<Trolley> GetTrolleys()
        {
            return db.Trolleys;
        }

        // GET: api/Trolleys/5
        [ResponseType(typeof(Trolley))]
        public IHttpActionResult GetTrolley(int id)
        {
            Trolley trolley = db.Trolleys.Find(id);
            if (trolley == null)
            {
                return NotFound();
            }

            return Ok(trolley);
        }


 


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool TrolleyExists(int id)
        {
            return db.Trolleys.Count(e => e.ID == id) > 0;
        }
    }
}