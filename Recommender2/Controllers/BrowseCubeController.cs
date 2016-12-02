using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Recommender2.ControllerEngine;

namespace Recommender2.Controllers
{
    public class BrowseCubeController : Controller
    {
        private readonly BrowseCubeControllerEngine _engine;

        public BrowseCubeController(BrowseCubeControllerEngine engine)
        {
            _engine = engine;
        }

        // GET: BrowseCube
        public ActionResult Index()
        {
            return View(_engine.GetDatasets());
        }

        // GET: BrowseCube/Details/id
        public ActionResult Details(int id)
        {
            var dataset = _engine.BrowseCube(id);
            if (dataset == null)
            {
                return HttpNotFound();
            }
            return View(dataset);
        }

        public ActionResult ShowChart(int datasetId, int selectedMeasureId, int xDimensionId, int legendDimensionId, Dictionary<int, Dictionary<int, bool>> dimensions)
        {
            var chart = _engine.ShowChart(datasetId, selectedMeasureId, xDimensionId, legendDimensionId, dimensions);
            if (chart == null)
            {
                return HttpNotFound();
            }
            return View("Details", chart);
        }

        // GET: BrowseCube/Details/id
        public FileResult Download(string file)
        {
            var fileBytes = _engine.GetFile(file);
            return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, Path.GetFileName(file));
        }
    }
}