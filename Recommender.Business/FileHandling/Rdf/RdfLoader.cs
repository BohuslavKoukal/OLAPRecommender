using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Recommender.Business.DTO;
using Recommender.Common.Constants;
using Recommender.Common.Helpers;
using VDS.RDF;

namespace Recommender.Business.FileHandling.Rdf
{
    public class RdfLoader
    {
        private readonly IGraph _dsdGraph;
        private readonly List<Triple> _components;
        private readonly RdfDsdConverter _dsdConverter;
        private readonly RdfObservationsConverter _observationsConverter;

        public RdfLoader(string dsdName, string dataName)
        {
            _dsdGraph = new Graph();
            _dsdGraph.LoadFromFile(dsdName);
            IGraph dataGraph = new Graph();
            dataGraph.LoadFromFile(dataName);
            var dsdNode = GetDsdNode();
            _components = GetComponents(dsdNode).ToList();
            _dsdConverter = new RdfDsdConverter(_dsdGraph);
            _observationsConverter = new RdfObservationsConverter(dataGraph);
        }

        public DataTable ConvertObservationsToDataTable(List<DimensionDto> dimensions, List<MeasureDto> measures)
        {
            return _observationsConverter.ConvertObservationsToDataTable(dimensions, measures);
        }

        public IEnumerable<DimensionDto> GetDimensions(string datasetName)
        {
            var dimensionTriples = GetDimensions(_components);
            var dimensions = _dsdConverter.ConvertToDimensionDtos(dimensionTriples);
            var dataFilledDimensions = _observationsConverter.FillDimensionsFromData(dimensions.ToList());
            return dataFilledDimensions.Select(d => {
                d.Name = d.Name.SafeName().ShortName(datasetName);
                return d;
            }).ToList();
        }

        public IEnumerable<MeasureDto> GetMeasures(string datasetName)
        {
            var measureTriples = GetMeasures(_components);
            return _dsdConverter.ConvertToMeasures(measureTriples).Select(m => {
                m.Name = m.Name.SafeName().ShortName(datasetName);
                return m;
            }).ToList();
        }

        public INode GetDsdNode()
        {
            return _dsdGraph.GetTriplesWithPredicateObject(RdfConstants.Type, RdfConstants.DsDefinition).Single().Subject;
        }

        public IEnumerable<Triple> GetComponents(INode dsdNode)
        {
            return _dsdGraph.GetTriplesWithSubjectPredicate(dsdNode, RdfConstants.Component);
        }

        public IEnumerable<Triple> GetMeasures(IEnumerable<Triple> components)
        {
            return GetBlankNodesByPredicate(components, RdfConstants.Measure);
        }

        public IEnumerable<Triple> GetDimensions(IEnumerable<Triple> components)
        {
            return GetBlankNodesByPredicate(components, RdfConstants.Dimension);
        }

        private IEnumerable<Triple> GetBlankNodesByPredicate(IEnumerable<Triple> components,
            INode predicateToMatch)
        {
            var ret = new List<Triple>();
            foreach (var component in components)
            {
                var triples = _dsdGraph.Triples
                    .Where(t => t.Subject.NodeType == NodeType.Blank &&
                    ((IBlankNode)t.Subject).InternalID == ((IBlankNode)component.Object).InternalID).ToList();
                var datasetAsAttachment =
                    triples.Where(
                        t =>
                            t.Predicate.Equals(RdfConstants.ComponentAttachment) &&
                            t.Object.Equals(RdfConstants.DataSet));
                // Add dimension only if it is connected to observations, not to dataset itself
                if (!datasetAsAttachment.Any())
                {
                    ret.AddRange(triples.Where(t => t.Predicate.Equals(predicateToMatch)));
                }
            }
            return ret;
        }
    }
}
