using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Recommender2.ControllerEngine;
using Recommender2.Validations;
using Recommender2.ViewModels;

namespace Recommender2.Controllers
{
    public class UploadController : Controller
    {
        private readonly UploadControllerEngine _controllerEngine;
        private readonly InputValidations _validations;

        public UploadController(UploadControllerEngine controllerEngine, InputValidations validations)
        {
            _controllerEngine = controllerEngine;
            _validations = validations;
        }

        public ActionResult Index()
        {
            return View(_controllerEngine.GetDataset());
        }

        public ActionResult UploadFile(string name, HttpPostedFileBase upload, string separator, bool keepFilePrivate)
        {
            try
            {
                _validations.DatasetNameIsValid(name);
                _validations.UploadedFileIsValid(upload);
            }
            catch (ValidationException e)
            {
                ModelState.AddModelError("Name", e.Message);
                return View("Index", _controllerEngine.GetDataset());
            }
            return View("CreateDataset", _controllerEngine.UploadFile(name, upload, separator, keepFilePrivate));
        }

        public ActionResult DefineDimensions(int id)
        {
            return View("CreateDataset", _controllerEngine.GetDataset(id));
        }


        [HttpPost]
        public ActionResult CreateDatasetManually(int id, AttributeViewModel[] attributes, string separator, string dateFormat)
        {
            // check if attributes are valid
            try
            {
                _validations.DatatypesAreValid(attributes, id, separator, dateFormat);
                _controllerEngine.CreateDataset(attributes, id, separator, dateFormat);
            }
            catch (ValidationException e)
            {
                ModelState.AddModelError("DataType", e.Message);
                return DefineDimensions(id);
            }
            return RedirectToAction("Index", "BrowseCube");
        }

        [HttpPost]
        public ActionResult CreateDatasetFromDsd(int id, HttpPostedFileBase upload)
        {
            // check if attributes are valid - what does it mean?
            // validovat datatypes - kdyz nejsou validni, vratit viewcko
            try
            {
                _validations.DsdIsValid(upload);
                _controllerEngine.CreateDataset(id, upload);
            }
            catch (ValidationException e)
            {
                ModelState.AddModelError("DataType", e.Message);
                return DefineDimensions(id);
            }
            catch (Exception e)
            {
                ModelState.AddModelError(string.Empty, "Uploaded file not correct: " + e.Message);
                return DefineDimensions(id);
            }
            return RedirectToAction("Index", "BrowseCube");
        }


    }
}