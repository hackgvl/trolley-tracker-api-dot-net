using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;

namespace TrolleyTracker.Controllers
{
    public class BulkUploadStopsController : Controller
    {
        // GET: BulkUploadStops
        public ActionResult Index()
        {
            return View();
        }

        // GET: BulkUploadStops/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: BulkUploadStops/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: BulkUploadStops/Create
        [CustomAuthorize(Roles = "RouteManagers")]
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                var jsonStops = collection.Get("JSONText");
                if (jsonStops != null)
                {
                    JavaScriptSerializer jss = new JavaScriptSerializer();
                    jss.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });

                    dynamic stopData = jss.Deserialize(jsonStops, typeof(object)) as dynamic;

                    var stops = stopData.TrolleyStops;
                    var nodeArray = stops.node;
                    var db = new TrolleyTracker.Models.TrolleyTrackerEntities();
                    int count = nodeArray.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var stop = nodeArray[i];
                        var lat = stop["lat"];
                        var lon = stop["lon"];
                        var name = stop["name"];

                        var dbStop = new TrolleyTracker.Models.Stop();
                        dbStop.Lat = Convert.ToDouble(lat);
                        dbStop.Lon = Convert.ToDouble(lon);
                        dbStop.Name = name;
                        dbStop.Description = name;
                        db.Stops.Add(dbStop);
                    }
                    db.SaveChanges();


                }

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //// GET: BulkUploadStops/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: BulkUploadStops/Edit/5
        //[HttpPost]
        //public ActionResult Edit(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add update logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: BulkUploadStops/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: BulkUploadStops/Delete/5
        //[HttpPost]
        //public ActionResult Delete(int id, FormCollection collection)
        //{
        //    try
        //    {
        //        // TODO: Add delete logic here

        //        return RedirectToAction("Index");
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
    }
}
