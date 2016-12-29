using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Business.Helpers;
using Recommender.Common.Enums;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business
{
    public interface IDataDecorator
    {
        void Insert(DatasetDto dataset);
        void PopulateDataset(int id, ICollection<MeasureDto> measures, ICollection<DimensionDto> dimensions, State state);
        string GetCsvFilePath(int id);
        List<DatasetDto> GetAllDatasets();
        DatasetDto GetDataset(string name);
        DatasetDto GetDataset(int id);
        List<DimensionDto> GetChildDimensions(int id);
        DimensionDto GetDimension(int id);
        MeasureDto GetMeasure(int id);
    }

    public class DataDecorator : IDataDecorator
    {
        private readonly IDataAccessLayer _dal;

        public DataDecorator(IDataAccessLayer dal)
        {
            _dal = dal;
        }

        public void Insert(DatasetDto dataset)
        {
            _dal.Insert(dataset.FromDto());
        }

        public void PopulateDataset(int id, ICollection<MeasureDto> measures, ICollection<DimensionDto> dimensions, State state)
        {
            _dal.PopulateDataset(id, measures.Select(m => m.FromDto()).ToList(), dimensions.Select(d => d.FromDto()).ToList(), state);
        }

        public string GetCsvFilePath(int id)
        {
            return _dal.GetCsvFilePath(id);
        }

        public List<DatasetDto> GetAllDatasets()
        {
            return _dal.GetAllDatasets().Select(d => d.ToDto()).ToList();
        }

        public DatasetDto GetDataset(string name)
        {
            return _dal.GetDataset(name).ToDto();
        }

        public DatasetDto GetDataset(int id)
        {
            return _dal.GetDataset(id).ToDto();
        }

        public List<DimensionDto> GetChildDimensions(int id)
        {
            return _dal.GetChildDimensions(id).Select(d => d.ToDto()).ToList();
        }

        public DimensionDto GetDimension(int id)
        {
            return _dal.GetDimension(id).ToDto();
        }

        public MeasureDto GetMeasure(int id)
        {
            return _dal.GetMeasure(id).ToDto();
        }
    }
}
