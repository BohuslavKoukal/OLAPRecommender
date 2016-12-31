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

        public ActionResult UploadFile(string name, HttpPostedFileBase upload, string separator)
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
            return View("CreateDataset", _controllerEngine.UploadFile(name, upload, separator));
        }

        public ActionResult DefineDimensions(int id)
        {
            return View("CreateDataset", _controllerEngine.GetDataset(id));
        }


        [HttpPost]
        public ActionResult CreateDataset(int id, AttributeViewModel[] attributes, string separator, string dateFormat)
        {
            // check if attributes are valid - what does it mean?
            // validovat datatypes - kdyz nejsou validni, vratit viewcko
            try
            {
                _validations.DatatypesAreValid(attributes, id, separator, dateFormat);
                _controllerEngine.CreateDataset(attributes, id, separator, dateFormat);
            }
            catch (ValidationException e)
            {
                ModelState.AddModelError("DataType", e.Message);
                return View();
            }
            return RedirectToAction("Index", "BrowseCube");
        }


    }
}