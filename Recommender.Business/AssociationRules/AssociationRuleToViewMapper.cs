using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Business.StarSchema;
using Recommender.Common.Constants;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public interface IAssociationRuleToViewMapper
    {
        Tuple<int, int> GetXAndLegendDimensionsId(AssociationRule rule, DimensionTree tree);
        string GetChartText(AssociationRule rule);
        IEnumerable<FlatDimensionDto> GetFilterValues(AssociationRule rule);
    }

    public class AssociationRuleToViewMapper : IAssociationRuleToViewMapper
    {
        private readonly IDataAccessLayer _data;
        private readonly IStarSchemaQuerier _querier;

        public AssociationRuleToViewMapper(IDataAccessLayer data, IStarSchemaQuerier querier)
        {
            _data = data;
            _querier = querier;
        }

        public Tuple<int, int> GetXAndLegendDimensionsId(AssociationRule rule, DimensionTree tree)
        {
            int xDimId, legendDimId;
            switch (rule.AntecedentValues.Count)
            {
                case 1:
                    xDimId = rule.AntecedentValues.Single().Dimension.Id;
                    legendDimId = rule.ConditionValues.Any() ? rule.ConditionValues.First().Dimension.Id : 0;
                    break;
                case 2:
                    var firstDimension = rule.AntecedentValues.ToList()[0].Dimension;
                    var secondDimension = rule.AntecedentValues.ToList()[1].Dimension;
                    var firstIsRoot = tree.IsRoot(firstDimension.Id);
                    var secondIsRoot = tree.IsRoot(secondDimension.Id);
                    if (firstIsRoot != secondIsRoot)
                    {
                        if (firstIsRoot)
                        {
                            xDimId = firstDimension.Id;
                            legendDimId = secondDimension.Id;
                        }
                        else
                        {
                            xDimId = secondDimension.Id;
                            legendDimId = firstDimension.Id;
                        }
                    }
                    else
                    {
                        xDimId = firstDimension.Id;
                        legendDimId = secondDimension.Id;
                    }
                    break;
                default:
                    throw new InvalidDataException($"Wrong count of antecedent values in rule {rule.Id}.");
            }
            return Tuple.Create(xDimId, legendDimId);
        }

        public string GetChartText(AssociationRule rule)
        {
            return $"Association rule: {rule.Text}";
        }

        public IEnumerable<FlatDimensionDto> GetFilterValues(AssociationRule rule)
        {
            var ret = new List<FlatDimensionDto>();
            foreach (var condition in rule.ConditionValues)
            {
                var dimension = _data.GetDimension(condition.Dimension.Id);
                var dimensionDto = new FlatDimensionDto
                {
                    Id = dimension.Id,
                    Name = dimension.Name,
                    DimensionValues = new List<DimensionValueDto>(),
                    DatasetName = rule.MiningTask.DataSet.Name
                };
                var allValues = _data.GetAllDimValues(dimension.Id);
                var dimValue = allValues.Single(dv => dv.Value == condition.Value);
                dimensionDto.DimensionValues.Add(new DimensionValueDto
                {
                    Dimension = dimensionDto,
                    Value = dimValue.Value,
                    Id = _querier.GetValuesOfDimension(dimensionDto, new Column
                    {
                        Name = Constants.String.Value,
                        Value = dimValue.Value
                    }).Single().Id
                });
                ret.Add(dimensionDto);
            }
            return ret;
        }
    }
}
