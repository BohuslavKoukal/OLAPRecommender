using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Recommender.Common.Enums;
using Recommender.Common.Helpers;

namespace Recommender2.ViewModels
{
    public class DatasetViewModel
    {
        public DatasetViewModel()
        {
            Datasets = new List<SingleDatasetViewModel>();
        }
        public List<SingleDatasetViewModel> Datasets { get; set; }
        public string Notification { get; set; }
    }

    public class SingleDatasetViewModelBase
    {
        public int Id { get; set; }
        [Required]
        [StringLength(50)]
        [Display(Name = "Dataset Name")]
        public string Name { get; set; }
    }

    public class SingleDatasetViewModel : SingleDatasetViewModelBase
    {
        public SingleDatasetViewModel()
        {
            Attributes = new List<AttributeViewModel>();
        }

        [Required]
        [Display(Name = "FileName")]
        public string FileName => Path.GetFileName(FilePath);

        [Required]
        [Display(Name = "FileName")]
        public string FilePath { get; set; }

        public FileType FileType
        {
            get
            {
                var fileType = FileName?.GetFileType();
                if (fileType != null) return (FileType) fileType;
                return FileType.Undefined;
            }
        }

        public State State { get; set; }

        public List<AttributeViewModel> Attributes { get; set; }
        public List<DimensionViewModel> Dimensions { get; set; }
        public List<MeasureViewModel> Measures { get; set; }
        public List<MiningTaskViewModel> MiningTasks { get; set; }

        public FilterViewModel Filter { get; set; }

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
        public List<SelectListItem> DataTypes => new List<SelectListItem>
        {
            new SelectListItem { Text = typeof(string).Name, Value = typeof(string).Name},
            new SelectListItem { Text = typeof(int).Name, Value = typeof(int).Name},
            new SelectListItem { Text = typeof(double).Name, Value = typeof(double).Name},
            new SelectListItem { Text = typeof(DateTime).Name, Value = typeof(DateTime).Name},
        };

        public string DataType { get; set; }

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
            new SelectListItem { Text = DateTime.Today.Date.ToString("M/d/yyyy"), Value = "M/d/yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("dd.MM.yyyy"), Value = "dd.MM.yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("yyyy.MM.dd"), Value = "yyyy.MM.dd"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("dd/MM/yyyy"), Value = "dd/MM/yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("yyyy/MM/dd"), Value = "yyyy/MM/dd"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("dd-MM-yyyy"), Value = "dd-MM-yyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("yyyy-MM-dd"), Value = "yyyy-MM-dd"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("ddMMyyyy"), Value = "ddMMyyyy"},
            new SelectListItem { Text = DateTime.Today.Date.ToString("yyyyMMdd"), Value = "yyyyMMdd"}
        };

        public string DateFormat { get; set; }
    }
}