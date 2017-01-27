﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Recommender.Common;
using Recommender.Data.Models;

namespace Recommender.Business.AssociationRules
{
    public class LispMinerConnector
    {
        private readonly IConfiguration _configuration;

        public LispMinerConnector(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendTask(string preprocessingPmml, string taskPmml)
        {
            // Send task to LM connect
        }

        public string GetTaskResultsFile(string taskName)
        {
            // Connect to LM-connect, try getting task results, save PMML to some folder
            var fileName = _configuration.GetPmmlFilesLocation() + taskName + ".pmml";
            return File.Exists(fileName) ? fileName : string.Empty;
        }

    }
}