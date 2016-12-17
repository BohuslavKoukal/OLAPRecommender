using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recommender2.Business.DTO;
using Recommender2.Business.Enums;

namespace Recommender2.ViewModels
{
    public class DatasetViewModel
    {
        public DatasetViewModel()
        {
            Datasets = new List<SingleDatasetViewModel>();
        }
        public List<SingleDatasetViewModel> Datasets { get; set; }
    }

    public class SingleDatasetViewModel
    {
        public SingleDatasetViewModel()
        {
            Attributes = new List<AttributeViewModel>();

        }
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Dataset Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "CSV FileName")]
        public string CsvFileName => Path.GetFileName(CsvFilePath);

        [Required]
        [Display(Name = "CSV FileName")]
        public string CsvFilePath { get; set; }

        public State State { get; set; }

        public List<AttributeViewModel> Attributes { get; set; }
        public List<DimensionViewModel> Dimensions { get; set; }
        public List<MeasureViewModel> Measures { get; set; }

        public List<SelectListItem> AttributesSelectList
        {
            get
            {
                return Attributes.Select(attribute => new SelectListItem
                {
                    Text = attribute.Name,
                    Value = attribute.Name
                }).ToList();
            }
        }

        public List<SelectListItem> MeasuresSelectList
        {
            get
            {
                return Measures.Select(measure => new SelectListItem
                {
                    Text = measure.Name,
                    Value = measure.Id.ToString()
                }).ToList();
            }
        }

        public List<SelectListItem> DimensionsSelectList { get; set; }

        [Required]
        [Display(Name = "Column separator")]
        public List<SelectListItem> SeparatorSelectList => new List<SelectListItem>
        {
            new SelectListItem { Text = ";", Value = ";"},
            new SelectListItem { Text = ",", Value = ","},
            new SelectListItem { Text = ".", Value = "."},
        };

        public string Separator { get; set; }

        [Required]
        [Display(Name = "Date format used in uploaded file")]
        public List<SelectListItem> DateFormatSelectList => new List<SelectListItem>
        {
            new SelectListItem { Text = DateTime.Today.Date.ToString("dd.MM.yyyy"), Value = "dd.MM.yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("MM.dd.yyyy"), Value = "MM.dd.yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("dd/MM/yyyy"), Value = "dd/MM/yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("MM/dd/yyyy"), Value = "MM/dd/yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("dd-MM-yyyy"), Value = "dd-MM-yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("MM-dd-yyyy"), Value = "MM-dd-yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("ddMMyyyy"), Value = "ddMMyyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("MMddyyyy"), Value = "MMddyyyy"}
        };

        public string DateFormat { get; set; }
    }
}