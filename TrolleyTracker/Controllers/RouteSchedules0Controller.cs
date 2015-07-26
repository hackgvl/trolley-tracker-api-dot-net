using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using TrolleyTracker.Models;
using TrolleyTracker.ViewModels;
using MvcSchedule.Objects;

namespace TrolleyTracker.Controllers
{
    public class RouteSchedules0Controller : Controller
    {
        // GET: RouteSchedules
        public ActionResult Index()
        {
            ViewBag.Message = "Edit Route Schedules";
            ViewBag.CssFile = Url.Content("~/Content/Site.css");

            var vm = new RouteScheduleViewModel();
            //using (var ctx = new TrolleyTrackerEntities())
            //{
            //    vm.RouteSchedules = ctx.RouteSchedules.ToList();
            //}
            vm.Options = new MvcScheduleGeneralOptions
            {
                Layout = LayoutEnum.Horizontal,
                SeparateDateHeader = false,
                FullTimeScale = false,
                TimeScaleInterval = 60,
                StartOfTimeScale = new TimeSpan(6, 0, 0),
                EndOfTimeScale = new TimeSpan(23, 59, 59),
                IncludeEndValue = true,
                ShowValueMarks = false,
                ItemCss = "normal",
                AlternatingItemCss = "normal2",
                RangeHeaderCss = "heading",
                TitleCss = "heading",
                BackgroundCss = "empty"
            };
            return View(vm);
        }

        // GET: RouteSchedules/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: RouteSchedules/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: RouteSchedules/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: RouteSchedules/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: RouteSchedules/Edit/5
        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        // GET: RouteSchedules/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: RouteSchedules/Delete/5
        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
