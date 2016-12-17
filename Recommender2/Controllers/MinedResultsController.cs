using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recommender2.ControllerEngine;

namespace Recommender2.Controllers
{
    public class MinedResultsController : Controller
    {
        private readonly BrowseCubeControllerEngine _engine;

        public MinedResultsController(BrowseCubeControllerEngine engine)
        {
            _engine = engine;
        }

        public ActionResult Index()
        {
            return View(_engine.GetDatasets());
        }

        // GET: MinedResults
        public ActionResult Mine(int id)
        {
            var dataset = _engine.BrowseCube(id);
            if (dataset == null)
            {
                return HttpNotFound();
            }
            return View(dataset);
        }
    }
}