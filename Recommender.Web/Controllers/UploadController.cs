using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Recommender.Common.Constants;
using Recommender.Web.ControllerEngine;
using Recommender.Web.Validations;
using Recommender.Web.ViewModels;
using Recommender2.Controllers;

namespace Recommender.Web.Controllers
{
    public class UploadController : BaseController
    {
        private readonly UploadControllerEngine _controllerEngine;
        private readonly IInputValidations _validations;

        public UploadController(UploadControllerEngine controllerEngine, IInputValidations validations)
        {
            _controllerEngine = controllerEngine;
            _validations = validations;
        }

        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult Index()
        {
            return View(_controllerEngine.GetDataset());
        }

        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult UploadFile(string name, HttpPostedFileBase upload, string separator)
        {
            try
            {
                _validations.DatasetNameIsValid(Identity, name);
                _validations.UploadedFileIsValid(upload);
            }
            catch (ValidationException e)
            {
                ModelState.AddModelError("Name", e.Message);
                return View("Index", _controllerEngine.GetDataset());
            }
            var viewModel = _controllerEngine.UploadFile(Identity, name, upload, separator);
            return RedirectToAction("CreateDataset", new { modelId = viewModel.Id, separatorString = separator });
        }

        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult CreateDataset(int modelId, string separatorString)
        {
            var model = _controllerEngine.GetDataset(modelId);
            model.Separator = separatorString;
            return View("CreateDataset", model);
        }

        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult DefineDimensions(int id, List<string> errors = null)
        {
            if (errors != null)
            {
                foreach (var error in errors)
                {
                    ModelState.AddModelError(string.Empty, error);
                }
            }
            return View("CreateDataset", _controllerEngine.GetDataset(id));
        }


        [HttpPost]
        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult CreateDatasetManually(int id, AttributeViewModel[] attributes, string separator, string dateFormat)
        {
            var errors = _validations.DatatypesAreValid(Identity, attributes, id, separator, dateFormat);
            if (errors.Any())
            {
                return DefineDimensions(id, errors);
            }
            _controllerEngine.CreateDataset(attributes, id, separator, dateFormat);
            return RedirectToAction("Index", "BrowseCube");
        }

        [HttpPost]
        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult CreateDatasetFromDsd(int id, HttpPostedFileBase upload)
        {
            try
            {
                _validations.DsdIsValid(upload);
                _controllerEngine.CreateDataset(Identity, id, upload);
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

        [Authorize(Roles = Roles.RoleUser)]
        public ActionResult Delete(int id)
        {
            var name = _controllerEngine.GetDatasetName(id);
            _controllerEngine.DeleteDataset(id);
            return RedirectToAction("Index", "BrowseCube", new { notification = $"Dataset {name} succesfully deleted."});
        }
        
    }
}