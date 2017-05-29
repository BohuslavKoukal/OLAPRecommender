using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recommender2.Controllers;

namespace Recommender.Web.Controllers
{
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "About OLAP Recommener";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Contact";
            return View();
        }
    }
}