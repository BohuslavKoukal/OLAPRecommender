using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Common.Constants;
using Recommender.Common.Helpers;
using Recommender.Data.Models;
using VDS.RDF;

namespace Recommender.Business.FileHandling.Rdf
{
    public class RdfDsdConverter
    {
        private readonly IGraph _graph;
        private readonly IGraph _graphHelper;

        public RdfDsdConverter(IGraph graph)
        {
            _graph = graph;
            _graphHelper = new Graph();

        }

        public IEnumerable<MeasureDto> ConvertToMeasures(IEnumerable<Triple> measureTriples)
        {
            var ret = measureTriples.Select(mt => mt.Object).Select(measure => new MeasureDto
            {
                Name = RdfConstants.GetNodeName(_graph, measure),
                RdfUri = measure.ToString(),
                Type = typeof(double)
            }).ToList();
            return ret;
        }

        /*
         * Convert triples to dimensions, set hierarchy, fill hierarchical values
         */
        public IEnumerable<DimensionDto> ConvertToDimensionDtos(IEnumerable<Triple> dimensionTriples)
        {
            // Get dimensions
            var dimTripleList = dimensionTriples.ToList();
            var dimList = ConvertTriplesToDimensions(dimTripleList).ToList();
            // Get dimensions values
            foreach (var triple in dimTripleList)
            {
                var correspondingDimension = dimList.Single(d => d.RdfUri.Equals(triple.Object.ToString()));
                if (correspondingDimension.FilledFrom == FilledFrom.Dsd)
                    correspondingDimension.DimensionValues = GetDimensionValues(correspondingDimension, triple).ToList();
            }
            var allDimValues = new List<DimensionValueDto>();
            foreach (var dimension in dimList)
            {
                allDimValues.AddRange(dimension.DimensionValues);
            }
            // Get hierarchy
            foreach (var dimension in dimList.Where(d => d.FilledFrom == FilledFrom.Dsd))
            {
                SetDimensionAndValuesHierarchy(dimension, allDimValues);
            }
            return dimList;
        }


        private IEnumerable<DimensionDto> ConvertTriplesToDimensions(IEnumerable<Triple> triples)
        {
            foreach (var triple in triples)
            {
                var range = _graph.GetTriplesWithSubjectPredicate(triple.Object, RdfConstants.Range).SingleOrDefault();
                Type dataType;
                var filledFrom = CanBeFilledFromDsd(triple) ? FilledFrom.Dsd : FilledFrom.Data;
                if (range != null)
                {
                    dataType = range.Object.Equals(RdfConstants.Interval) ? typeof(DateTime) : typeof(string);
                }
                else
                {
                    dataType = typeof(string);
                }
                yield return new TreeDimensionDto
                {
                    RdfUri = triple.Object.ToString(),
                    Type = dataType,
                    DimensionValues = new List<DimensionValueDto>(),
                    Name = RdfConstants.GetNodeName(_graph, triple.Object),
                    FilledFrom = filledFrom
                };
            }
        }

        private bool CanBeFilledFromDsd(Triple triple)
        {
            var range = _graph.GetTriplesWithSubjectPredicate(triple.Object, RdfConstants.Range).SingleOrDefault();
            if (range != null)
            {
                var codelist = _graph.GetTriplesWithSubjectPredicate(triple.Object, RdfConstants.Codelist).SingleOrDefault();
                if (codelist != null)
                {
                    var definedCodelistValues = _graph.GetTriplesWithPredicateObject(RdfConstants.InScheme, codelist.Object);
                    if (definedCodelistValues.Any())
                        return true;
                }
            }
            return false;
        }

        private IEnumerable<DimensionValueDto> GetDimensionValues(DimensionDto dimension, Triple triple)
        {
            var rangeTriple = _graph.GetTriplesWithSubjectPredicate(triple.Object, RdfConstants.Range).SingleOrDefault();
            if (rangeTriple.Object.Equals(RdfConstants.Concept))
            {
                var codeList = _graph.GetTriplesWithSubjectPredicate(triple.Object, RdfConstants.Codelist).Single().Object;
                var values = _graph.GetTriplesWithPredicateObject(RdfConstants.InScheme, codeList);
                foreach (var value in values)
                {
                    yield return new DimensionValueDto
                    {
                        Dimension = dimension,
                        Value = RdfConstants.GetNodeName(_graph, value.Subject),
                        RdfUri = value.Subject.ToString()
                    };
                }
            }
        }

        private void SetDimensionAndValuesHierarchy(DimensionDto dimension, List<DimensionValueDto> allDimensionsValues)
        {
            foreach (var value in dimension.DimensionValues)
            {
                var broaderValueTriple = _graph.GetTriplesWithSubjectPredicate
                    (_graphHelper.CreateUriNode(UriFactory.Create(value.RdfUri)), RdfConstants.Broader).SingleOrDefault();
                if (broaderValueTriple != null)
                {
                    var broaderValue = allDimensionsValues.Single(dv => dv.RdfUri.Equals(broaderValueTriple.Object.ToString()));
                    var parentDimension = broaderValue.Dimension;
                    if (dimension.ParentDimension == null)
                    {
                        dimension.ParentDimension = parentDimension;
                    }
                    else
                    {
                        if (dimension.ParentDimension != parentDimension)
                            throw new InvalidDataException($"Invalid hierarchy. Not clear if parent of dimension {dimension.Name} is {dimension.ParentDimension.Name} or {parentDimension.Name}.");
                    }
                    value.ParentDimensionValue = broaderValue;
                }
            }
        }
    }
}
