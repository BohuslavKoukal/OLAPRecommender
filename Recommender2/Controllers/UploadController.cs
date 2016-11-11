using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using Recommender2.Business.Validations;
using Recommender2.ControllerEngine;
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
            return View();
        }

        public ActionResult UploadFile(string name, HttpPostedFileBase upload)
        {
            try
            {
                _validations.DatasetNameIsValid(name);
                _validations.UploadedFileIsValid(upload);
            }
            catch (ValidationException e)
            {
                ModelState.AddModelError("Name", e.Message);
                return View("Index");
            }
            return View("CreateDataset", _controllerEngine.UploadFile(name, upload));
        }

        [HttpPost]
        public ActionResult CreateDataset(int id, AttributeViewModel[] attributes)
        {
            // check if attributes are valid - what does it mean?
            // validovat datatypes - kdyz nejsou validni, vratit viewcko
            try
            {
                _validations.DatatypesAreValid(attributes, id);
                _controllerEngine.CreateDataset(id, attributes);
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