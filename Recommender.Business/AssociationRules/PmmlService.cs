using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Recommender.Common.Enums;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public class PmmlService
    {
        private readonly XmlDocument doc;

        public PmmlService(string resultsPmmlFileName)
        {
            doc = new XmlDocument();
            doc.Load(resultsPmmlFileName);
        }

        public PmmlService()
        {
        }

        public string GetPreprocessingPmml(MiningTask task)
        {
            return string.Empty;
        }

        public string GetMiningTaskPmml(MiningTask task)
        {
            return string.Empty;
        }

        public List<AssociationRule> GetRules(List<DimensionValue> dimensionValues, List<Measure> measures)
        {
            var ret = new List<AssociationRule>();
            var associationRules = doc.SelectNodes("//*[local-name()='AssociationRule']");
            var dbas = doc.SelectNodes("//*[local-name()='DBA']").Cast<XmlNode>().ToList();
            foreach (XmlNode rule in associationRules)
            {
                var antecedentName = rule.Attributes["antecedent"].InnerText;
                var succedentName = rule.Attributes["consequent"].InnerText;
                var conditionName = rule.Attributes["condition"].InnerText;
                var antecedentDba = dbas.Single(dba => dba.Attributes["id"].InnerText == antecedentName);
                var succedentDba = dbas.Single(dba => dba.Attributes["id"].InnerText == succedentName);
                var conditionDba = dbas.Single(dba => dba.Attributes["id"].InnerText == conditionName);
                var antecedentText =
                    antecedentDba.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("Text")).InnerText;
                var succedentText =
                    succedentDba.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("Text")).InnerText;
                var conditionText =
                    conditionDba.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("Text")).InnerText;
                var aadValue = Convert.ToDouble(rule.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("IMValue") && childNode.Attributes["name"].InnerText == "AAD").InnerText);
                var baseValue = Convert.ToDouble(rule.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("IMValue") && childNode.Attributes["name"].InnerText == "BASE").InnerText);
                var ruleText = rule.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("Text")).InnerText;
                var assocRule = new AssociationRule
                {
                    Aad = aadValue,
                    Base = baseValue,
                    Text = ruleText,
                    AntecedentValues = GetLiteralConjunction(antecedentText, dimensionValues),
                    ConditionValues = GetLiteralConjunction(conditionText, dimensionValues),
                    SuccedentMeasure = GetSuccedentMeasure(succedentText, measures)
                };
                ret.Add(assocRule);
            }
            return ret;
        }

        public TimeSpan GetTaskDuration()
        {
            var taskDuration = doc.SelectNodes("//*[local-name()='TaskDuration']").Cast<XmlNode>().Single().InnerText;
            var durationParts = taskDuration.Split(' ');
            var hours = Convert.ToInt32(durationParts[0].Replace("h", string.Empty));
            var minutes = Convert.ToInt32(durationParts[1].Replace("m", string.Empty));
            var seconds = Convert.ToInt32(durationParts[2].Replace("s", string.Empty));
            return new TimeSpan(hours, minutes, seconds);
        }

        public int GetNumberOfVerifications()
        {
            return Convert.ToInt32(doc.SelectNodes("//*[local-name()='NumberOfVerifications']").Cast<XmlNode>().Single().InnerText);
        }

        private List<DimensionValue> GetLiteralConjunction(string conjunctionText, List<DimensionValue> dimensionValues)
        {
            var ret = new List<DimensionValue>();
            var conjunctionParts = conjunctionText.Split(new [] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var conjunctionPart in conjunctionParts)
            {
                var split = conjunctionPart.Split('(');
                var dimensionName = split[0].TrimStart(' ').TrimEnd(' ');
                var valueName = split[1].TrimStart(' ').TrimEnd(' ', ')');
                ret.Add(dimensionValues.Single(dv => dv.Dimension.Name == dimensionName && dv.Value == valueName));
            }
            return ret;
        }

        private Measure GetSuccedentMeasure(string succedentText, List<Measure> measures)
        {
            // Succedent text is supposed to be in form Units(>= [500;1000]) or Units([500;1000])

            //var sign = succedentParts[1].Split(' ')[0];
            //var signEnum = Sign.EquivalentTo;
            //if(sign.StartsWith(">")) signEnum = Sign.Higher;
            //else if (sign.StartsWith("<")) signEnum = Sign.Lower;
            //var boundaries = signEnum == Sign.EquivalentTo ? succedentParts[1].Split(';') : succedentParts[1].Split(' ')[1].Split(';');
            //var lowerBoundary = Convert.ToInt32(boundaries[0].Remove(0, 1));
            //var higherBoundary = Convert.ToInt32(boundaries[1].Remove(boundaries[1].Length - 2, 2));
            var succedentParts = succedentText.Split('(');
            return measures.Single(m => m.Name == succedentParts[0]);
        }

    }
}
