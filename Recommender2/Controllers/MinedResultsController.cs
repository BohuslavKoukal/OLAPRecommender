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
        private readonly MinedResultsControllerEngine _engine;
        private readonly BrowseCubeControllerEngine _browseCubeEgine;

        public MinedResultsController(MinedResultsControllerEngine engine, BrowseCubeControllerEngine browseCubeEgine)
        {
            _engine = engine;
            _browseCubeEgine = browseCubeEgine;
        }

        public ActionResult Index()
        {
            return View(_browseCubeEgine.GetDatasets());
        }

        // GET: MinedResults
        public ActionResult Mine(int id)
        {
            var dataset = _browseCubeEgine.GetDataset(id);
            if (dataset == null)
            {
                return HttpNotFound();
            }
            return View(dataset);
        }

        public ActionResult MineRules(int id, string name, double baseQ, double aadQ, Dictionary<int, Dictionary<int, bool>> dimensions)
        {
            _engine.MineRules(id, name, baseQ, aadQ, dimensions);
            var model = _browseCubeEgine.GetDatasets();
            model.Notification =
                "Your task was succesfully sent to LispMiner. You will be notified once the mining is finished.";
            return View("Index", model);
        }

        public ActionResult Details(int id)
        {
            return View(_engine.GetDetails(id));
        }

    }
}