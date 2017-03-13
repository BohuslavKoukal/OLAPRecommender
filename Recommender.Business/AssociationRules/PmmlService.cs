using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using Recommender.Business.DTO;
using Recommender.Common;
using Recommender.Common.Helpers;
using Recommender.Data.Extensions;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public class PmmlService
    {
        private readonly XmlDocument _doc;
        private readonly IConfiguration _configuration;
        private readonly Tuple<string, string> _xmlns = Tuple.Create("xmlns", "http://www.dmg.org/PMML-4_0");
        private readonly Tuple<string, string> _xmlnsXsi = Tuple.Create("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
        private readonly Tuple<string, string> _xmlnsPmml = Tuple.Create("xmlns:pmml", "http://www.dmg.org/PMML-4_0");
        private readonly Tuple<string, string> _xmlnsSchemalocation = Tuple.Create("xsi:schemaLocation", "http://www.dmg.org/PMML-4_0 http://sewebar.vse.cz/schemas/PMML4.0+GUHA0.1.xsd");
        private readonly Tuple<string, string> _pmml = Tuple.Create("pmml", "http://www.dmg.org/PMML-4_0");
        private readonly Tuple<string, string> _guha = Tuple.Create("guha", "http://keg.vse.cz/ns/GUHA0.1rev1");

        public PmmlService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public PmmlService(IConfiguration configuration, string resultsPmmlFileName)
        {
            _configuration = configuration;
            _doc = new XmlDocument();
            _doc.Load(resultsPmmlFileName);
        }

        public XmlDocument GetPreprocessingPmml(MiningTask task, List<Discretization> discretizations, int rowCount)
        {
            XmlDocument doc = new XmlDocument();
            var pmml = GetSchemaDefinition(doc);
            GetHeaders(pmml, doc, task.DataSet.Name + "metabase", task.DataSet.Name + "view");
            CreateMiningBuildTask(pmml, doc, task);
            CreateDataDictionary(pmml, doc, task);
            CreateTransformationDictionary(pmml, doc, task, discretizations, rowCount);
            doc.Save(_configuration.GetPmmlFilesLocation() + task.Name + "preprocessing.pmml");
            return doc;
        }

        public XmlDocument GetTaskPmml(MiningTask task, List<EquivalencyClass> eqClasses, int rowCount)
        {
            var document = GetMiningTaskPmml(task, eqClasses, rowCount);
            document.Save(_configuration.GetPmmlFilesLocation() + task.Name + "task.pmml");
            return document;
        }

        #region MiningTask
        private XmlDocument GetMiningTaskPmml(MiningTask task, List<EquivalencyClass> eqClasses, int rowCount)
        {
            XmlDocument doc = new XmlDocument();
            var pmml = GetSchemaDefinition(doc);
            GetHeaders(pmml, doc, task.DataSet.Name + "metabase", task.DataSet.Name + "view");
            var gam = GetGuhaAssociationModel(task, doc, pmml);
            var ts = GetTaskSetting(doc, gam);
            GetLispMinerExtension(doc, ts);
            GetBbaSettings(task, doc, ts);
            GetDbaSettings(task, doc, ts, eqClasses);
            CreateAntecedentConsequentAndConditionSettings(task, ts, doc);
            CreateInterestMeasureSettings(task, ts, doc, rowCount);
            CreatePmmlMiningSchema(doc, gam, task);
            return doc;
        }

        private XmlElement GetGuhaAssociationModel(MiningTask task, XmlDocument doc, XmlElement root)
        {
            var gam = (XmlElement)root.AppendChild(doc.CreateElement(_guha.Item1, "AssociationModel", _guha.Item2));
            
            gam.SetAttribute("xsi:schemaLocation", "http://keg.vse.cz/ns/GUHA0.1rev1 http://sewebar.vse.cz/schemas/GUHA0.1rev1.xsd");
            //gam.SetAttribute("xmlns:guha", "http://keg.vse.cz/ns/GUHA0.1rev1");
            gam.SetAttribute("modelName", task.Name);
            gam.SetAttribute("functionName", "associationRules");
            gam.SetAttribute("algorithmName", "4ft");
            return gam;
        }

        private XmlElement GetTaskSetting(XmlDocument doc, XmlElement guhaAssocModel)
        {
            var ts = (XmlElement) guhaAssocModel.AppendChild(doc.CreateElement("TaskSetting"));
            return ts;
        }

        private void GetLispMinerExtension(XmlDocument doc, XmlElement taskSetting)
        {
            var lmex = (XmlElement)taskSetting.AppendChild(doc.CreateElement("Extension"));
            lmex.SetAttribute("name", "LISp-Miner");
            var taskGroup = (XmlElement)lmex.AppendChild(doc.CreateElement("TaskGroup"));
            taskGroup.InnerText = "Default Task Group";
            var ftMissingType = (XmlElement)lmex.AppendChild(doc.CreateElement("FTMissingsType"));
            ftMissingType.InnerText = "Delete";
            var fTTaskParamProlong100AFlag = (XmlElement)lmex.AppendChild(doc.CreateElement("FTTaskParamProlong100AFlag"));
            fTTaskParamProlong100AFlag.InnerText = "Yes";
            var fTTaskParamProlong100SFlag = (XmlElement)lmex.AppendChild(doc.CreateElement("FTTaskParamProlong100SFlag"));
            fTTaskParamProlong100SFlag.InnerText = "Yes";
            var fTTaskParamPrimeCheckMinLen = (XmlElement)lmex.AppendChild(doc.CreateElement("FTTaskParamPrimeCheckMinLen"));
            fTTaskParamPrimeCheckMinLen.InnerText = "Yes";
            var fTTaskParamPrimeCheck = (XmlElement)lmex.AppendChild(doc.CreateElement("FTTaskParamPrimeCheck"));
            fTTaskParamPrimeCheck.InnerText = "Yes";
            var fTTaskParamIncludeSymetricFlag = (XmlElement)lmex.AppendChild(doc.CreateElement("FTTaskParamIncludeSymetricFlag"));
            fTTaskParamIncludeSymetricFlag.InnerText = "Yes";
            var hypothesesCountMax = (XmlElement)lmex.AppendChild(doc.CreateElement("HypothesesCountMax"));
            hypothesesCountMax.InnerText = "1000";
        }

        private void GetBbaSettings(MiningTask task, XmlDocument doc, XmlElement taskSetting)
        {
            var bbaSettings = (XmlElement)taskSetting.AppendChild(doc.CreateElement("BBASettings"));
            foreach (var dimension in task.DataSet.GetNonDateDimensions())
            {
                CreateBbaSetting(bbaSettings, doc, dimension.Name, dimension.GetBbaId(), dimension.GetQualifiedNameValue(), "Subset", 1);
            }
            foreach (var measure in task.DataSet.Measures)
            {
                CreateBbaSetting(bbaSettings, doc, measure.Name, measure.GetBbaId(), measure.GetQualifiedName(), "Interval", 3);
            }
        }

        private void CreateBbaSetting(XmlElement bbaSettings, XmlDocument doc, string name, string id, string nameValue, string coefType, int maxLength)
        {
            var bbaSetting = (XmlElement)bbaSettings.AppendChild(doc.CreateElement("BBASetting"));
            bbaSetting.SetAttribute("id", id);
            var naame = (XmlElement)bbaSetting.AppendChild(doc.CreateElement("Name"));
            naame.InnerText = name;
            var fieldRef = (XmlElement)bbaSetting.AppendChild(doc.CreateElement("FieldRef"));
            fieldRef.InnerText = nameValue;
            var coefficient = (XmlElement)bbaSetting.AppendChild(doc.CreateElement("Coefficient"));
            var type = (XmlElement)coefficient.AppendChild(doc.CreateElement("Type"));
            type.InnerText = coefType;
            var minimalLength = (XmlElement)coefficient.AppendChild(doc.CreateElement("MinimalLength"));
            minimalLength.InnerText = "1";
            var maximalLength = (XmlElement)coefficient.AppendChild(doc.CreateElement("MaximalLength"));
            maximalLength.InnerText = maxLength.ToString();
        }

        private void GetDbaSettings(MiningTask task, XmlDocument doc, XmlElement taskSetting, List<EquivalencyClass> equivalencyClasses)
        {
            var dbaSettings = (XmlElement)taskSetting.AppendChild(doc.CreateElement("DBASettings"));
            foreach (var dimension in task.DataSet.GetNonDateDimensions())
            {
                CreateLiteralDbaSetting(dbaSettings, doc, dimension.Name, dimension.GetDbaId(), dimension.GetBbaId(),
                    equivalencyClasses.SingleOrDefault(ec => ec.Dimensions.Select(d => d.Name).Contains(dimension.Name)));
            }
            foreach (var measure in task.DataSet.Measures)
            {
                CreateLiteralDbaSetting(dbaSettings, doc, measure.Name, measure.GetDbaId(), measure.GetBbaId());
            }

            CreateConjunctionDbaSetting(dbaSettings, doc, task.GetAntecedentId(), task.GetAntecedentName(), task.GetAntecedentDimensions(), 1, 2);
            CreateConjunctionDbaSetting(dbaSettings, doc, task.GetSuccedentId(), task.GetSuccedentName(), task.DataSet.Measures.ToList(), 1, 1);
            CreateConjunctionDbaSetting(dbaSettings, doc, task.GetConditionId(), task.GetConditionName(), task.GetConditionDimensions(), task.ConditionRequired ? 1 : 0, task.GetConditionDimensions().Count);

            CreateBagDbaSetting(doc, dbaSettings, task.GetAntecedentBagId(), task.GetAntecedentName(), task.GetAntecedentId(), 1, 1);
            CreateBagDbaSetting(doc, dbaSettings, task.GetSuccedentBagId(), task.GetSuccedentName(), task.GetSuccedentId(), 1, 1);
            CreateBagDbaSetting(doc, dbaSettings, task.GetConditionBagId(), task.GetConditionName(), task.GetConditionId(), 1, 1);
        }

        private void CreateConjunctionDbaSetting(XmlElement dbaSettings, XmlDocument doc, string id, string name, List<Dimension> dimensions, int minLength, int maxLength)
        {
            var dbaSetting = CreateFirstPartOfConjunctionDbaSetting(dbaSettings, doc, id, name);
            foreach (var dimension in dimensions)
            {
                var baSettingRef = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("BASettingRef"));
                baSettingRef.InnerText = dimension.GetDbaId();
            }
            CreateLastPartOfConjunctionDbaSetting(dbaSetting, doc, minLength, maxLength);
        }

        private void CreateConjunctionDbaSetting(XmlElement dbaSettings, XmlDocument doc, string id, string name, List<Measure> measures, int minLength, int maxLength)
        {
            var dbaSetting = CreateFirstPartOfConjunctionDbaSetting(dbaSettings, doc, id, name);
            foreach (var measure in measures)
            {
                var baSettingRef = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("BASettingRef"));
                baSettingRef.InnerText = measure.GetDbaId();
            }
            CreateLastPartOfConjunctionDbaSetting(dbaSetting, doc, minLength, maxLength);
        }

        private XmlElement CreateFirstPartOfConjunctionDbaSetting(XmlElement dbaSettings, XmlDocument doc, string id, string name)
        {
            var dbaSetting = (XmlElement)dbaSettings.AppendChild(doc.CreateElement("DBASetting"));
            dbaSetting.SetAttribute("id", id);
            dbaSetting.SetAttribute("type", "Conjunction");
            var nameElement = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("Name"));
            nameElement.InnerText = name;
            return dbaSetting;
        }

        private void CreateLastPartOfConjunctionDbaSetting(XmlElement dbaSetting, XmlDocument doc, int minLength, int maxLength)
        {
            var minimalLength = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("MinimalLength"));
            minimalLength.InnerText = minLength.ToString();
            var maximalLength = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("MaximalLength"));
            maximalLength.InnerText = maxLength.ToString();
        }

        private void CreateLiteralDbaSetting(XmlElement dbaSettings, XmlDocument doc, string name, string id, string bbaId, EquivalencyClass eqClass = null)
        {
            var dbaSetting = (XmlElement)dbaSettings.AppendChild(doc.CreateElement("DBASetting"));
            dbaSetting.SetAttribute("id", id);
            dbaSetting.SetAttribute("type", "Literal");
            var naame = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("Name"));
            naame.InnerText = name;
            var baSettingRef = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("BASettingRef"));
            baSettingRef.InnerText = bbaId;
            var literalSign = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("LiteralSign"));
            literalSign.InnerText = "Positive";
            var literalType = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("LiteralType"));
            literalType.InnerText = "Basic";
            if (eqClass != null)
            {
                var equivalenceClass = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("EquivalenceClass"));
                equivalenceClass.InnerText = eqClass.Name;
            }
        }

        private void CreateBagDbaSetting(XmlDocument doc, XmlElement dbaSettings, string id, string name, string baRef, int minLength, int maxLength)
        {
            var dbaSetting = (XmlElement)dbaSettings.AppendChild(doc.CreateElement("DBASetting"));
            dbaSetting.SetAttribute("id", id);
            dbaSetting.SetAttribute("type", "Conjunction");
            var naame = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("Name"));
            naame.InnerText = name;
            var baSettingsRef = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("BASettingRef"));
            baSettingsRef.InnerText = baRef;
            var minimalLength = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("MinimalLength"));
            minimalLength.InnerText = minLength.ToString();
            var maximalLength = (XmlElement)dbaSetting.AppendChild(doc.CreateElement("MaximalLength"));
            maximalLength.InnerText = maxLength.ToString();
        }

        private void CreateAntecedentConsequentAndConditionSettings(MiningTask task, XmlElement taskSetting, XmlDocument doc)
        {
            var antecedentSetting = (XmlElement)taskSetting.AppendChild(doc.CreateElement("AntecedentSetting"));
            antecedentSetting.InnerText = task.GetAntecedentBagId();
            var consequentSetting = (XmlElement)taskSetting.AppendChild(doc.CreateElement("ConsequentSetting"));
            consequentSetting.InnerText = task.GetSuccedentBagId();
            var conditionSetting = (XmlElement)taskSetting.AppendChild(doc.CreateElement("ConditionSetting"));
            conditionSetting.InnerText = task.GetConditionBagId();
        }

        private void CreateInterestMeasureSettings(MiningTask task, XmlElement taskSetting, XmlDocument doc, int rowCount)
        {
            var interestMeasureSetting = (XmlElement)taskSetting.AppendChild(doc.CreateElement("InterestMeasureSetting"));
            var baseMeasureThreshold = (XmlElement)interestMeasureSetting.AppendChild(doc.CreateElement("InterestMeasureThreshold"));
            baseMeasureThreshold.SetAttribute("id", "1");
            var baseMeasure = (XmlElement)baseMeasureThreshold.AppendChild(doc.CreateElement("InterestMeasure"));
            baseMeasure.InnerText = "BASE";
            var baseCompareType = (XmlElement)baseMeasureThreshold.AppendChild(doc.CreateElement("CompareType"));
            baseCompareType.InnerText = "Greater than or equal";
            var baseThreshold = (XmlElement)baseMeasureThreshold.AppendChild(doc.CreateElement("Threshold"));
            baseThreshold.InnerText = Math.Floor(rowCount * task.Base * 0.01).ToString(CultureInfo.InvariantCulture);
            baseThreshold.SetAttribute("type", "Abs");

            var aadMeasureThreshold = (XmlElement)interestMeasureSetting.AppendChild(doc.CreateElement("InterestMeasureThreshold"));
            aadMeasureThreshold.SetAttribute("id", "2");
            var aadMeasure = (XmlElement)aadMeasureThreshold.AppendChild(doc.CreateElement("InterestMeasure"));
            aadMeasure.InnerText = "AAD";
            var aadCompareType = (XmlElement)aadMeasureThreshold.AppendChild(doc.CreateElement("CompareType"));
            aadCompareType.InnerText = "Greater than or equal";
            var aadThreshold = (XmlElement)aadMeasureThreshold.AppendChild(doc.CreateElement("Threshold"));
            aadThreshold.InnerText = task.Aad.ToString(CultureInfo.InvariantCulture);
            aadThreshold.SetAttribute("type", "Abs");
        }

        private void CreatePmmlMiningSchema(XmlDocument doc, XmlElement guhaAssocModel, MiningTask task)
        {
            var miningSchema = (XmlElement)guhaAssocModel.AppendChild(doc.CreateElement(_pmml.Item1, "MiningSchema", _pmml.Item2));
            foreach (var dimension in task.DataSet.GetNonDateDimensions())
            {
                var miningField = (XmlElement)miningSchema.AppendChild(doc.CreateElement(_pmml.Item1, "MiningField", _pmml.Item2));
                miningField.SetAttribute("name", dimension.GetQualifiedNameValue());
            }
            foreach (var measure in task.DataSet.Measures)
            {
                var miningField = (XmlElement)miningSchema.AppendChild(doc.CreateElement(_pmml.Item1, "MiningField", _pmml.Item2));
                miningField.SetAttribute("name", measure.GetQualifiedName());
            }
        }

        #endregion

        #region PreprocessingPMML

        private XmlElement GetSchemaDefinition(XmlDocument doc)
        {
            XmlDeclaration declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declaration);
            XmlProcessingInstruction pi = doc.CreateProcessingInstruction("oxygen", "SCHSchema=\"http://sewebar.vse.cz/schemas/GUHARestr0_1.sch\"");
            doc.AppendChild(pi);
            var pmml = (XmlElement)doc.AppendChild(doc.CreateElement("PMML"));
            //XmlSchema schema = new XmlSchema();
            //schema.Namespaces.Add(_xmlns.Item1, _xmlns.Item2);
            //schema.Namespaces.Add(_xmlnsXsi.Item1, _xmlnsXsi.Item2);
            //schema.Namespaces.Add(_xmlnsPmml.Item1, _xmlnsPmml.Item2);
            //schema.Namespaces.Add(_xmlnsSchemalocation.Item1, _xmlnsSchemalocation.Item2);
            //doc.Schemas.Add(schema);
            pmml.SetAttribute("version", "4.0");
            pmml.SetAttribute("xmlns", "http://www.dmg.org/PMML-4_0");
            pmml.SetAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            pmml.SetAttribute("xmlns:pmml", "http://www.dmg.org/PMML-4_0");
            pmml.SetAttribute("xsi:schemaLocation", "http://www.dmg.org/PMML-4_0 http://sewebar.vse.cz/schemas/PMML4.0+GUHA0.1.xsd");
            return pmml;
        }

        private void GetHeaders(XmlElement rootElement, XmlDocument doc, string metabaseName, string datasetName)
        {
            var header = (XmlElement)rootElement.AppendChild(doc.CreateElement("Header"));
            header.SetAttribute("copyright", "Copyright (c) KIZI UEP");
            var metabase = (XmlElement)header.AppendChild(doc.CreateElement("Extension"));
            metabase.SetAttribute("name", "metabase");
            metabase.SetAttribute("value", metabaseName);
            var dataset = (XmlElement)header.AppendChild(doc.CreateElement("Extension"));
            dataset.SetAttribute("name", "dataset");
            dataset.SetAttribute("value", datasetName);
            var subsystem = (XmlElement)header.AppendChild(doc.CreateElement("Extension"));
            subsystem.SetAttribute("name", "subsystem");
            subsystem.SetAttribute("value", "4ft-Miner");
            var module = (XmlElement)header.AppendChild(doc.CreateElement("Extension"));
            module.SetAttribute("name", "module");
            module.SetAttribute("value", "4ftResult.exe");
            var format = (XmlElement)header.AppendChild(doc.CreateElement("Extension"));
            format.SetAttribute("name", "format");
            format.SetAttribute("value", "4ftMiner.Task");
            var timestamp = (XmlElement)header.AppendChild(doc.CreateElement("Timestamp"));
            timestamp.InnerText = DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss", CultureInfo.InvariantCulture);
        }

        private void CreateMiningBuildTask(XmlElement rootElement, XmlDocument doc, MiningTask task)
        {
            var mbt = (XmlElement)rootElement.AppendChild(doc.CreateElement("MiningBuildTask"));
            var extension = (XmlElement)mbt.AppendChild(doc.CreateElement("Extension"));
            extension.SetAttribute("name", "DatabaseDictionary");
            var table = (XmlElement)extension.AppendChild(doc.CreateElement("Table"));
            table.SetAttribute("name", task.DataSet.Name + "view");
            var columns = (XmlElement)table.AppendChild(doc.CreateElement("Columns"));
            foreach (var dimension in task.DataSet.GetNonDateDimensions())
            {
                CreateMiningBuildTaskColumn(columns, doc, dimension.GetQualifiedNameValue(), dimension.Type.ToLispName());
            }
            foreach (var measure in task.DataSet.Measures)
            {
                CreateMiningBuildTaskColumn(columns, doc, measure.GetQualifiedName(), measure.Type.ToLispName());
            }
            CreateMiningBuildTaskColumn(columns, doc, $"{task.DataSet.Name}Id", "integer");
            var pk = (XmlElement)table.AppendChild(doc.CreateElement("PrimaryKey"));
            var pkColumn = (XmlElement)pk.AppendChild(doc.CreateElement("Column"));
            pkColumn.SetAttribute("name", $"{task.DataSet.Name}Id");
            pkColumn.SetAttribute("primaryKeyPosition", "0");
        }

        private void CreateMiningBuildTaskColumn(XmlElement columns, XmlDocument doc, string name, string datatype)
        {
            var column = (XmlElement)columns.AppendChild(doc.CreateElement("Column"));
            column.SetAttribute("name", name);
            column.SetAttribute("dataType", datatype);
        }

        private void CreateDataDictionary(XmlElement rootElement, XmlDocument doc, MiningTask task)
        {
            var dd = (XmlElement)rootElement.AppendChild(doc.CreateElement("DataDictionary"));
            var attrCount = task.DataSet.GetNonDateDimensions().Count + task.DataSet.Measures.Count + 1;
            dd.SetAttribute("numberOfFields", attrCount.ToString());
            foreach (var dimension in task.DataSet.GetNonDateDimensions())
            {
                CreateDataDictionaryDataField(dd, doc, dimension.GetQualifiedNameValue(), dimension.Type.ToLispName(), "categorical");
            }
            foreach (var measure in task.DataSet.Measures)
            {
                CreateDataDictionaryDataField(dd, doc, measure.GetQualifiedName(), measure.Type.ToLispName(), "continuous");
            }
            CreateDataDictionaryDataField(dd, doc, "Id", "integer", "continuous");
        }

        private void CreateDataDictionaryDataField(XmlElement dataDictionary, XmlDocument doc, string name, string type, string optype)
        {
            var df = (XmlElement)dataDictionary.AppendChild(doc.CreateElement("DataField"));
            df.SetAttribute("name", name);
            df.SetAttribute("optype", optype);
            df.SetAttribute("dataType", type);
        }

        private void CreateTransformationDictionary(XmlElement rootElement, XmlDocument doc, MiningTask task, List<Discretization> discretizations, int rowCount)
        {
            var td = (XmlElement)rootElement.AppendChild(doc.CreateElement("TransformationDictionary"));
            // Dimensions
            foreach (var dimension in task.DataSet.GetNonDateDimensions())
            {
                var dimensionValueName = dimension.GetQualifiedNameValue();
                var dimInlineTable = SetupInlineTable(td, doc, dimensionValueName, dimension.Type.ToLispName(), "ordinal");
                foreach (var value in dimension.DimensionValues)
                {
                    var row = dimInlineTable.AppendChild(doc.CreateElement("row"));
                    var col = row.AppendChild(doc.CreateElement("column"));
                    col.InnerText = value.Value;
                    var field = row.AppendChild(doc.CreateElement("field"));
                    field.InnerText = value.Value;
                }
            }
            // Id
            var idInlineTable = SetupInlineTable(td, doc, $"{task.DataSet.Name}Id", "string", "ordinal");
            for (int i = 1; i <= rowCount; i++)
            {
                var row = idInlineTable.AppendChild(doc.CreateElement("row"));
                var col = row.AppendChild(doc.CreateElement("column"));
                col.InnerText = i.ToString();
                var field = row.AppendChild(doc.CreateElement("field"));
                field.InnerText = i.ToString();
            }
            // Measures
            foreach (var measure in task.DataSet.Measures)
            {
                var measureValueName = measure.GetQualifiedName();
                var measureDerivedField = SetupDerivedField(td, doc, measureValueName, measure.Type.ToLispName(), "continuous");
                var discretize = (XmlElement)measureDerivedField.AppendChild(doc.CreateElement("Discretize"));
                discretize.SetAttribute("field", measureValueName);
                var measureDisc = discretizations.Single(d => d.Measure.Id == measure.Id);
                foreach (var bin in measureDisc.Bins)
                {
                    var discretizeBin = (XmlElement)discretize.AppendChild(doc.CreateElement("DiscretizeBin"));
                    discretizeBin.SetAttribute("binValue", bin.Name);
                    var interval = (XmlElement)discretizeBin.AppendChild(doc.CreateElement("Interval"));
                    interval.SetAttribute("closure", "closedOpen");
                    interval.SetAttribute("leftMargin", bin.LeftMargin.ToString());
                    interval.SetAttribute("rightMargin", bin.RightMargin.ToString());
                }
            }
        }

        private XmlElement SetupInlineTable(XmlElement transformationDictionary, XmlDocument doc, string valueName, string type, string optype)
        {
            var df = SetupDerivedField(transformationDictionary, doc, valueName, type, optype);
            var mapValues = (XmlElement)df.AppendChild(doc.CreateElement("MapValues"));
            mapValues.SetAttribute("outputColumn", valueName);
            var fcp = (XmlElement)mapValues.AppendChild(doc.CreateElement("FieldColumnPair"));
            fcp.SetAttribute("column", valueName);
            fcp.SetAttribute("field", valueName);
            var inlineTable = (XmlElement)mapValues.AppendChild(doc.CreateElement("InlineTable"));
            return inlineTable;
        }

        private XmlElement SetupDerivedField(XmlElement transformationDictionary, XmlDocument doc, string valueName, string type, string optype)
        {
            var df = (XmlElement)transformationDictionary.AppendChild(doc.CreateElement("DerivedField"));
            df.SetAttribute("name", valueName);
            df.SetAttribute("dataType", type);
            df.SetAttribute("optype", optype);
            var parentGroup = (XmlElement)df.AppendChild(doc.CreateElement("Extension"));
            parentGroup.SetAttribute("name", "ParentGroup");
            parentGroup.SetAttribute("value", "Root attribute group");
            var shortName = (XmlElement)df.AppendChild(doc.CreateElement("Extension"));
            shortName.SetAttribute("name", "ShortName");
            shortName.SetAttribute("value", valueName);
            return df;
        }
        #endregion

        #region ParsingRules
        public List<AssociationRule> GetRules(List<DimensionValue> dimensionValues, List<Measure> measures)
        {
            var ret = new List<AssociationRule>();
            var associationRules = _doc.SelectNodes("//*[local-name()='AssociationRule']");
            var dbas = _doc.SelectNodes("//*[local-name()='DBA']").Cast<XmlNode>().ToList();
            foreach (XmlNode rule in associationRules)
            {
                var antecedentName = rule.Attributes["antecedent"].InnerText;
                var succedentName = rule.Attributes["consequent"].InnerText;
                
                var antecedentDba = dbas.Single(dba => dba.Attributes["id"].InnerText == antecedentName);
                var succedentDba = dbas.Single(dba => dba.Attributes["id"].InnerText == succedentName);
                var antecedentText =
                    antecedentDba.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("Text")).InnerText;
                var succedentText =
                    succedentDba.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("Text")).InnerText;
                var aadString =
                    rule.ChildNodes.Cast<XmlNode>()
                        .Single(
                            childNode =>
                                childNode.LocalName.Equals("IMValue") &&
                                childNode.Attributes["name"].InnerText == "AAD")
                        .InnerText;
                var aadValue = Convert.ToDouble(aadString, NumberFormatInfo.InvariantInfo);
                var baseString =
                    rule.ChildNodes.Cast<XmlNode>()
                        .Single(
                            childNode =>
                                childNode.LocalName.Equals("IMValue") &&
                                childNode.Attributes["name"].InnerText == "BASE")
                        .InnerText;
                var baseValue = Convert.ToDouble(baseString, NumberFormatInfo.InvariantInfo);
                var ruleText = rule.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("Text")).InnerText;
                var assocRule = new AssociationRule
                {
                    Text = ruleText,
                    AntecedentValues = GetLiteralConjunction(antecedentText, dimensionValues),
                    Succedents = new List<Succedent> { GetSuccedent(succedentText, measures, aadValue, baseValue) } ,
                    ConditionValues = new List<DimensionValue>()
                };
                var conditionExists = rule.Attributes["condition"] != null;
                if (conditionExists)
                {
                    var conditionName = rule.Attributes["condition"].InnerText;
                    var conditionDba = dbas.Single(dba => dba.Attributes["id"].InnerText == conditionName);
                    var conditionText =
                    conditionDba.ChildNodes.Cast<XmlNode>().Single(childNode => childNode.LocalName.Equals("Text")).InnerText;
                    assocRule.ConditionValues = GetLiteralConjunction(conditionText, dimensionValues);
                }
                ret.Add(assocRule);
            }
            return ret;
        }

        public TimeSpan GetTaskDuration()
        {
            var taskDuration = _doc.SelectNodes("//*[local-name()='TaskDuration']").Cast<XmlNode>().Single().InnerText;
            var durationParts = taskDuration.Split(' ');
            var hours = Convert.ToInt32(durationParts[0].Replace("h", string.Empty));
            var minutes = Convert.ToInt32(durationParts[1].Replace("m", string.Empty));
            var seconds = Convert.ToInt32(durationParts[2].Replace("s", string.Empty));
            return new TimeSpan(hours, minutes, seconds);
        }

        public int GetNumberOfVerifications()
        {
            return Convert.ToInt32(_doc.SelectNodes("//*[local-name()='NumberOfVerifications']").Cast<XmlNode>().Single().InnerText);
        }

        private List<DimensionValue> GetLiteralConjunction(string conjunctionText, List<DimensionValue> dimensionValues)
        {
            var ret = new List<DimensionValue>();
            var conjunctionParts = conjunctionText.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var conjunctionPart in conjunctionParts)
            {
                var split = conjunctionPart.Split('(');
                var dimensionName = split[0].TrimStart(' ').TrimEnd(' ');
                var valueName = split[1].TrimStart(' ').TrimEnd(' ', ')');
                ret.Add(dimensionValues.Single(dv => dv.Dimension.GetQualifiedNameValue() == dimensionName && dv.Value == valueName));
            }
            return ret;
        }

        private Succedent GetSuccedent(string succedentText, List<Measure> measures, double aadValue, double baseValue)
        {
            var succedentParts = succedentText.Split('(');
            return new Succedent
            {
                Aad = aadValue,
                Base = baseValue,
                Measure = measures.Single(m => m.Name == succedentParts[0]),
                SuccedentText = succedentText
            };
        }
        #endregion

    }
}
