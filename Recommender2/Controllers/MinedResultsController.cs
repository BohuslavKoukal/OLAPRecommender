using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recommender2.ControllerEngine;
using Recommender2.Validations;
using Recommender2.ViewModels;

namespace Recommender2.Controllers
{
    public class MinedResultsController : Controller
    {
        private readonly MinedResultsControllerEngine _engine;
        private readonly BrowseCubeControllerEngine _browseCubeEgine;
        private readonly InputValidations _validations;

        public MinedResultsController(MinedResultsControllerEngine engine, BrowseCubeControllerEngine browseCubeEgine, InputValidations validations)
        {
            _engine = engine;
            _browseCubeEgine = browseCubeEgine;
            _validations = validations;
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
            try
            {
                _validations.TaskNameIsValid(model.TaskName);
            }
            catch (ValidationException e)
            {
                ModelState.AddModelError("Name", e.Message);
                return View("Mine", _engine.GetMiningViewModel(model.Id));
            }
            _engine.MineRules(model.Id, model);
            var model2 = _browseCubeEgine.GetDatasets();
            model2.Notification =
                "Your task was succesfully sent to LispMiner. You can see its state below.";
            return View("Index", model2);
        }

        public ActionResult Details(int id)
        {
            return View(_engine.GetDetails(id));
        }

    }
}