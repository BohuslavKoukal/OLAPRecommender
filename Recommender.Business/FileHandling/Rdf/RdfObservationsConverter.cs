using System;
using System.Collections.Generic;
using System.Data;
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
    public class RdfObservationsConverter
    {
        private readonly IGraph _graph;
        private readonly IGraph _graphHelper;

        public RdfObservationsConverter(IGraph graph)
        {
            _graph = graph;
            _graphHelper = new Graph();

        }

        public List<DimensionDto> FillDimensionsFromData(List<DimensionDto> dimensions)
        {
            var dimensionsToFill = dimensions.Where(d => d.FilledFrom == FilledFrom.Data);
            foreach (var dimension in dimensionsToFill)
            {
                dimension.DimensionValues = GetDimensionValues(dimension);
            }
            return dimensions;
        }

        private List<DimensionValueDto> GetDimensionValues(DimensionDto dimension)
        {
            var values = new List<Tuple<string, string>>();
            var observations = _graph.GetTriplesWithPredicateObject(RdfConstants.Type, RdfConstants.Observation);
            foreach (var observation in observations)
            {
                // Find triple where observation subject is subject and dimension is predicate
                var observationOfDimension = _graph.GetTriplesWithSubjectPredicate
                    (observation.Subject, _graphHelper.CreateUriNode(UriFactory.Create(dimension.RdfUri))).SingleOrDefault();
                if (observationOfDimension == null)
                {
                    throw new InvalidDataException($"Observation {observation.Subject} has no value of dimension {dimension.Name}. ");
                }
                var objectUri = observationOfDimension.Object.ToString();
                if (!values.Select(v => v.Item1).Contains(objectUri))
                    values.Add(Tuple.Create(objectUri, RdfConstants.GetNodeName(_graph, objectUri)));
            }
            return values.Select(value => new DimensionValueDto
            {
                Dimension = dimension,
                RdfUri = value.Item1,
                Value = value.Item2
            }).ToList();
        }

        public DataTable ConvertObservationsToDataTable(List<DimensionDto> dimensions, List<MeasureDto> measures)
        {
            var table = GetDataTableStructure(dimensions, measures);
            // list vsech triplu, ktere jsou observation
            var observations = _graph.GetTriplesWithPredicateObject(RdfConstants.Type, RdfConstants.Observation);
            // pro kazdou observation vlozit radek do datatable a ziskat list triplu, kde je observation subjektem
            var dimAndMeasuresList = new List<DimensionOrMeasureDto>();
            dimAndMeasuresList.AddRange(dimensions);
            dimAndMeasuresList.AddRange(measures);
            foreach (var observation in observations)
            {
                var triplesWhereObservationIsSubject = _graph.GetTriplesWithSubject(observation.Subject).ToList();
                var dict = GetDimensionAndMeasureDictionary(dimensions, measures);
                foreach (var dimOrMeasure in dimAndMeasuresList)
                {
                    var dimOrMeasureUri = _graphHelper.CreateUriNode(UriFactory.Create(dimOrMeasure.RdfUri));
                    var observedValue =
                        triplesWhereObservationIsSubject.WithPredicate(dimOrMeasureUri).SingleOrDefault();
                    if (observedValue != null)
                        dict[dimOrMeasure.Name] = RdfConstants.GetNodeName(_graph, observedValue.Object);
                }
                var objectArrayToAdd = new List<object>();
                foreach (var dimOrMeasure in dict)
                {
                    objectArrayToAdd.Add(dimOrMeasure.Value);
                }
                var row = table.NewRow();
                row.ItemArray = objectArrayToAdd.ToArray();
                table.Rows.Add(row);
            }
            return table;
        }

        private DataTable GetDataTableStructure(IEnumerable<DimensionDto> dimensions, IEnumerable<MeasureDto> measures)
        {
            var table = new DataTable();
            foreach (var dimension in dimensions)
            {
                if (dimension.Type == typeof(string))
                    table.Columns.Add(dimension.Name, typeof(string));
                else
                {
                    table.Columns.Add(dimension.Name, typeof(DateTime));
                }
            }
            foreach (var measure in measures)
            {
                table.Columns.Add(measure.Name, typeof(double));
            }
            return table;
        }

        private Dictionary<string, object> GetDimensionAndMeasureDictionary(IEnumerable<DimensionDto> dimensions, IEnumerable<MeasureDto> measures)
        {
            var domDict = new Dictionary<string, object>();
            foreach (var dimension in dimensions)
            {
                domDict.Add(dimension.Name, null);
            }
            foreach (var measure in measures)
            {
                domDict.Add(measure.Name, null);
            }
            return domDict;
        }
    }
}
