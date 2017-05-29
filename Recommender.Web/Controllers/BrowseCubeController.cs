using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Recommender.Common.Constants;
using Recommender.Web.ControllerEngine;
using Recommender.Web.ViewModels;
using Recommender2.Controllers;

namespace Recommender.Web.Controllers
{
    public class BrowseCubeController : BaseController
    {
        private readonly BrowseCubeControllerEngine _engine;

        public BrowseCubeController(BrowseCubeControllerEngine engine)
        {
            _engine = engine;
        }

        // GET: BrowseCube
        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult Index()
        {
            return View(_engine.GetDatasets(User.Identity.GetUserId()));
        }

        // GET: BrowseCube/Details/id
        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult Details(int id)
        {
            var dataset = _engine.BrowseCube(id);
            if (dataset == null)
            {
                return HttpNotFound();
            }
            return View(dataset);
        }

        [HttpGet]
        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult ShowChart(int ruleId)
        {
            var chart = _engine.ShowChart(ruleId);
            if (chart == null)
            {
                return HttpNotFound();
            }
            return View("Details", chart);
        }

        [HttpPost]
        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult ShowChart(int datasetId, BrowseCubeViewModel model)
        {
            var chart = _engine.ShowChart(datasetId, model.SelectedMeasureId, model.XDimensionId, model.LegendDimensionId, model.Group, model.Dataset.Filter);
            if (chart == null)
            {
                return HttpNotFound();
            }
            return View("Details", chart);
        }

        // GET: BrowseCube/Details/id
        [Authorize(Roles = Roles.RoleUser)]
        public FileResult Download(string file)
        {
            var fileBytes = _engine.GetFile(file);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(file));
        }
    }
}