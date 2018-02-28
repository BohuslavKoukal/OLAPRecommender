using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Recommender.Common;
using Recommender.Common.Enums;
using Recommender.Data.DataAccess;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public class LispMinerConnector
    {
        private readonly IConfiguration _configuration;
        private readonly IDataAccessLayer _data;

        public LispMinerConnector(IConfiguration configuration, IDataAccessLayer data)
        {
            _configuration = configuration;
            _data = data;
        }

        public bool SendPreprocessing(string userId, MiningTask task, XmlDocument preprocessingPmml, HttpClient client = null)
        {
            if(client == null) client = new HttpClient();
            using (client)
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                var method = new HttpMethod("PATCH");
                var stringContent = new StringContent(preprocessingPmml.OuterXml);
                var request = new HttpRequestMessage(method, GetSendPreprocessingAddress(task.DataSet.MinerId))
                {
                    Content = stringContent
                };
                try
                {
                    var response = client.SendAsync(request).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == HttpStatusCode.InternalServerError || response.StatusCode == HttpStatusCode.NotFound)
                    {
                        if (responseString.Contains("Requested LISpMiner with ID") && responseString.Contains("does not exists"))
                        {
                            var minerId = PmmlService.GetMinerId(GetMinerId(userId, task));
                            _data.SetMinerId(userId, task.DataSet.Id, minerId);
                            task.DataSet.MinerId = minerId;
                            return SendPreprocessing(userId, task, preprocessingPmml, null);
                        }
                        _data.SetTaskState(userId, task.Id, (int)TaskState.Failed, responseString);
                        return false;
                    }
                }
                catch (TaskCanceledException e)
                {
                    _data.SetTaskState(userId, task.Id, (int)TaskState.Failed, "Connection to Lisp Miner timed out after 10 minutes.");
                    return false;
                }
                
            }
            _data.SetPreprocessed(task.DataSet.Id);
            return true;
        }

        public string GetMinerId(string userId, MiningTask task)
        {
            string responseString = null;
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                var stringContent = new StringContent("type=MySQLConnection&metabase=&server=localhost&database=recommendercubes&username=root&password=root",
                                    Encoding.UTF8,
                                    "application/x-www-form-urlencoded");
                try
                {
                    var response = client.PostAsync(GetSendRegistrationAddress(), stringContent).Result;
                    responseString = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        File.WriteAllText(GetDatasetRegistrationFileName(task.DataSet.Id), responseString);
                    }
                }
                catch (TaskCanceledException e)
                {
                    File.WriteAllText(GetDatasetRegistrationFileName(task.DataSet.Id), "Connection to Lisp Miner timed out after 10 minutes.");
                }

            }
            return responseString;
        }

        public bool SendTask(string userId, MiningTask task, XmlDocument taskPmml, XmlDocument preprocessingPmml)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(10);
                var stringContent = new StringContent(taskPmml.OuterXml);
                try
                {
                    var response = client.PostAsync(GetSendTaskAddress(task.DataSet.MinerId), stringContent).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        if (responseString.Contains("Requested LISpMiner with ID") && responseString.Contains("does not exists"))
                        {
                            var minerId = PmmlService.GetMinerId(GetMinerId(userId, task));
                            _data.SetMinerId(userId, task.DataSet.Id, minerId);
                            task.DataSet.MinerId = minerId;
                            var preprocessed = SendPreprocessing(userId, task, preprocessingPmml);
                            return preprocessed && SendTask(userId, task, taskPmml, preprocessingPmml);
                        }
                        _data.SetTaskState(userId, task.Id, (int) TaskState.Failed, responseString);
                        return false;
                    }
                    else
                    {
                        File.WriteAllText(GetTaskResultsFileName(task.Name), responseString);
                        return true;
                    }
                }
                catch (TaskCanceledException e)
                {
                    _data.SetTaskState(userId, task.Id, (int)TaskState.Failed, "Connection to Lisp Miner timed out after 10 minutes.");
                    return false;
                }
                
            }
        }

        

        private string GetSendTaskAddress(string minerId)
        {
            return $"{_configuration.GetLispMinerServer()}miners/{minerId}/tasks/task?alias=&template=4ftMiner.Task.Template.PMML";
        }

        private string GetSendRegistrationAddress()
        {
            return $"{_configuration.GetLispMinerServer()}miners";
        }

        private string GetSendPreprocessingAddress(string minerId)
        {
            return $"{_configuration.GetLispMinerServer()}miners/{minerId}";
        }

        public string GetTaskResultsFile(string taskName)
        {
            // Connect to LM-connect, try getting task results, save PMML to some folder
            var fileName = GetTaskResultsFileName(taskName);
            return File.Exists(fileName) ? fileName : string.Empty;
        }

        private string GetTaskResultsFileName(string taskName)
        {
            return _configuration.GetPmmlFilesLocation() + taskName + "results.pmml";
        }

        private string GetDatasetRegistrationFileName(int datasetId)
        {
            return _configuration.GetPmmlFilesLocation() + datasetId + "registration.xml";
        }

    }
}
