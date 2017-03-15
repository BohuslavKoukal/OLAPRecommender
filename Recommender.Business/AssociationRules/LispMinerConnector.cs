﻿using System;
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

        public bool SendPreprocessing(MiningTask task, XmlDocument preprocessingPmml)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromHours(1);
                var method = new HttpMethod("PATCH");
                var stringContent = new StringContent(preprocessingPmml.OuterXml);
                var request = new HttpRequestMessage(method, GetSendPreprocessingAddress())
                {
                    Content = stringContent
                };
                try
                {
                    var response = client.SendAsync(request).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == HttpStatusCode.InternalServerError || response.StatusCode == HttpStatusCode.NotFound)
                    {
                        _data.SetTaskState(task.Id, (int)TaskState.Failed, responseString);
                        return false;
                    }
                }
                catch (TaskCanceledException e)
                {
                    _data.SetTaskState(task.Id, (int)TaskState.Failed, "Connection timed out after 1 hour.");
                    return false;
                }
                
            }
            _data.SetPreprocessed(task.DataSet.Id);
            return true;
        }

        public bool SendTask(MiningTask task, XmlDocument taskPmml)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromHours(1);
                var stringContent = new StringContent(taskPmml.OuterXml);
                try
                {
                    var response = client.PostAsync(GetSendTaskAddress(), stringContent).Result;
                    var responseString = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == HttpStatusCode.InternalServerError)
                    {
                        _data.SetTaskState(task.Id, (int) TaskState.Failed, responseString);
                        return false;
                    }
                    else
                    {
                        _data.SetTaskState(task.Id, (int) TaskState.Finished);
                        File.WriteAllText(GetTaskResultsFileName(task.Name), responseString);
                        return true;
                    }
                }
                catch (TaskCanceledException e)
                {
                    _data.SetTaskState(task.Id, (int)TaskState.Failed, "Connection timed out after 1 hour.");
                    return false;
                }
                
            }
        }

        

        private string GetSendTaskAddress()
        {
            return $"{_configuration.GetLispMinerServer()}miners/{_configuration.GetMinerId()}/tasks/task?alias=&template=4ftMiner.Task.Template.PMML";
        }

        private string GetSendPreprocessingAddress()
        {
            return $"{_configuration.GetLispMinerServer()}miners/{_configuration.GetMinerId()}";
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

    }
}
