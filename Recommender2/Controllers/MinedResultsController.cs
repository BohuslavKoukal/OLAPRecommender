using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recommender2.ControllerEngine;
using Recommender2.ViewModels;

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
            var dataset = _engine.GetMiningViewModel(id);
            if (dataset == null)
            {
                return HttpNotFound();
            }
            return View(dataset);
        }

        public ActionResult MineRules(MiningViewModel model)
        {
            _engine.MineRules(model.Id, model);
            var model2 = _browseCubeEgine.GetDatasets();
            model2.Notification =
                "Your task was succesfully sent to LispMiner. You will be notified once the mining is finished.";
            return View("Index", model2);
        }

        public ActionResult Details(int id)
        {
            return View(_engine.GetDetails(id));
        }

    }
}