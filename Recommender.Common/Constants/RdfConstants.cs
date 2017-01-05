using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDS.RDF;

namespace Recommender.Common.Constants
{
    public static class RdfConstants
    {
        public static readonly IUriNode Observation;
        public static readonly IUriNode Type;
        public static readonly IUriNode Label;
        public static readonly IUriNode DsDefinition;
        public static readonly IUriNode Component;
        public static readonly IUriNode Dimension;
        public static readonly IUriNode Measure;
        public static readonly IUriNode Broader;
        public static readonly IUriNode Interval;
        public static readonly IUriNode Range;
        public static readonly IUriNode Concept;
        public static readonly IUriNode Codelist;
        public static readonly IUriNode InScheme;
        public static readonly IUriNode PrefLabel;
        public static readonly IUriNode ComponentAttachment;
        public static readonly IUriNode DataSet;
        public static readonly string GregorianDay;
        public static readonly string GregorianMonth;
        public static readonly string GregorianYear;

        static RdfConstants()
        {
            var graph = new Graph();
            Observation = graph.CreateUriNode(UriFactory.Create("http://purl.org/linked-data/cube#Observation"));
            Type = graph.CreateUriNode(UriFactory.Create("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));
            Label = graph.CreateUriNode(UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#label"));
            DsDefinition = graph.CreateUriNode(UriFactory.Create("http://purl.org/linked-data/cube#DataStructureDefinition"));
            Component = graph.CreateUriNode(UriFactory.Create("http://purl.org/linked-data/cube#component"));
            Dimension = graph.CreateUriNode(UriFactory.Create("http://purl.org/linked-data/cube#dimension"));
            Measure = graph.CreateUriNode(UriFactory.Create("http://purl.org/linked-data/cube#measure"));
            Broader = graph.CreateUriNode(UriFactory.Create("http://www.w3.org/2004/02/skos/core#broader"));
            Interval = graph.CreateUriNode(UriFactory.Create("http://reference.data.gov.uk/def/intervals/Interval"));
            Range = graph.CreateUriNode(UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#range"));
            Concept = graph.CreateUriNode(UriFactory.Create("http://www.w3.org/2004/02/skos/core#Concept"));
            Codelist = graph.CreateUriNode(UriFactory.Create("http://purl.org/linked-data/cube#codeList"));
            InScheme = graph.CreateUriNode(UriFactory.Create("http://www.w3.org/2004/02/skos/core#inScheme"));
            PrefLabel = graph.CreateUriNode(UriFactory.Create("https://www.w3.org/2009/08/skos-reference/skos.html#prefLabel"));
            ComponentAttachment = graph.CreateUriNode(UriFactory.Create("http://purl.org/linked-data/cube#componentAttachment"));
            DataSet = graph.CreateUriNode(UriFactory.Create("http://purl.org/linked-data/cube#DataSet"));
            GregorianDay = "http://reference.data.gov.uk/id/gregorian-day/";
            GregorianMonth = "http://reference.data.gov.uk/id/gregorian-month/";
            GregorianYear = "http://reference.data.gov.uk/id/gregorian-year/";
        }

        public static string GetNodeName(IGraph graph, string nodeUri)
        {
            var graphHelper = new Graph();
            return GetNodeName(graph, graphHelper.CreateUriNode(UriFactory.Create(nodeUri)));
        }

        public static string GetNodeName(IGraph graph, INode objectNode)
        {
            var nodeName = GetNodeNameWithLanguageFlag(graph, objectNode);
            if (nodeName.Length > 2 && nodeName.Split('@').Last().Length == 2)
            {
                return nodeName.Remove(nodeName.Length - 3, 3);
            }
            return nodeName;
        }

        private static string GetNodeNameWithLanguageFlag(IGraph graph, INode objectNode)
        {
            var prefLabelTriples = graph.GetTriplesWithSubjectPredicate(objectNode, PrefLabel).ToList();
            var labelTriples = graph.GetTriplesWithSubjectPredicate(objectNode, Label).ToList();
            switch (prefLabelTriples.Count)
            {
                case 0:
                    switch (labelTriples.Count)
                    {
                        case 0:
                            var objectNodeString = objectNode.ToString();
                            return objectNodeString.Contains("^^") ? objectNodeString.Split('^').First() : objectNodeString.Split(new[] { '/', '#' }).Last();
                        case 1:
                            return labelTriples.Single().Object.ToString();
                        default:
                            var enLabelTriple = GetLabelTripleInLanguage(labelTriples, "en");
                            if (enLabelTriple != null) return enLabelTriple.Object.ToString();
                            var csLabelTriple = GetLabelTripleInLanguage(labelTriples, "cs");
                            if (csLabelTriple != null) return csLabelTriple.Object.ToString();
                            return labelTriples.First().Object.ToString();
                    }
                case 1:
                    return prefLabelTriples.Single().Object.ToString();
                default:
                    var enPrefLabelTriple = GetLabelTripleInLanguage(prefLabelTriples, "en");
                    if (enPrefLabelTriple != null) return enPrefLabelTriple.Object.ToString();
                    var csPrefLabelTriple = GetLabelTripleInLanguage(prefLabelTriples, "cs");
                    if (csPrefLabelTriple != null) return csPrefLabelTriple.Object.ToString();
                    return labelTriples.First().Object.ToString();
            }
        }

        public static Triple GetLabelTripleInLanguage(List<Triple> triples, string language)
        {
            return triples.SingleOrDefault(lt => lt.Object.ToString().Split('@').Last().Equals(language));
        }
    }
}
