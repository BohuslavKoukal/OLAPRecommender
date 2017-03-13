using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Business.StarSchema;
using Recommender.Data.Extensions;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public class DataDiscretizator
    {
        private readonly IStarSchemaQuerier _querier;

        public DataDiscretizator(IStarSchemaQuerier starSchemaQuerier)
        {
            _querier = starSchemaQuerier;
        }

        public List<Discretization> GetDiscretizations(Dataset dataset, int rowCount)
        {
            var binCount = GetBinCount(rowCount);
            var ret = new List<Discretization>();
            foreach (var measure in dataset.Measures)
            {
                var disc = new Discretization {Measure = measure};
                var orderedValues = _querier.GetOrderedMeasureValues(measure);
                var step = rowCount/binCount;
                var currentLeftMargin = (int)Math.Floor(orderedValues[0]);
                for (int i = 1; i <= binCount; i++)
                {
                    var bin = new DiscretizeBin
                    {
                        LeftMargin = currentLeftMargin,
                        RightMargin = (int) Math.Floor(orderedValues[i*step])
                    };
                    currentLeftMargin = bin.RightMargin;
                    disc.Bins.Add(bin);
                }
                ret.Add(disc);
            }
            return ret;
        }

        public List<EquivalencyClass> GetEquivalencyClasses(DimensionTree tree)
        {
            var equivalencyClasses = new List<EquivalencyClass>();
            foreach (var rootDim in tree.RootDimensions)
            {
                var eqC = new EquivalencyClass
                {
                    Name = "eq_class_" + rootDim.Name,
                    Dimensions = new List<DimensionDto>()
                };
                eqC.Dimensions.AddRange(rootDim.GetSubtreeDimensionDtos());
                equivalencyClasses.Add(eqC);
            }
            return equivalencyClasses;
        }

        private int GetBinCount(int rowCount)
        {
            if (rowCount < 100)
            {
                return 5;
            }
            else if (rowCount < 1000)
            {
                return 10;
            }
            else if (rowCount < 10000)
            {
                return 15;
            }
            return 20;
        }
    }
}
