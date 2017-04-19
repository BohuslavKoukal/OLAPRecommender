using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recommender.Web.ControllerEngine;
using Recommender.Web.Validations;
using Recommender.Web.ViewModels;

namespace Recommender.Web.Controllers
{
    public class MinedResultsController : Controller
    {
        private readonly MinedResultsControllerEngine _engine;
        private readonly BrowseCubeControllerEngine _browseCubeEgine;
        private readonly IInputValidations _validations;

        public MinedResultsController(MinedResultsControllerEngine engine, BrowseCubeControllerEngine browseCubeEgine, IInputValidations validations)
        {
            _engine = engine;
            _browseCubeEgine = browseCubeEgine;
            _validations = validations;
        }

        public ActionResult Index(string notification = null)
        {
            var model = _browseCubeEgine.GetDatasets();
            model.Notification = notification;
            return View(model);
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
            var modelNotification =
                "Your task was succesfully sent to LispMiner. You can see its state below.";
            return RedirectToAction("Index", new { notification = modelNotification });
        }

        public ActionResult Details(int id)
        {
            return View(_engine.GetDetails(id));
        }

    }
}