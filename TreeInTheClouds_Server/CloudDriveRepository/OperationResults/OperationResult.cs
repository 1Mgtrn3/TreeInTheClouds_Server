using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeInTheClouds_Server.CloudDriveRepository.Enums;

namespace TreeInTheClouds_Server.CloudDriveRepository.OperationResults
{
    public class OperationResult
    {
        public OperationStatusCode operationResult { get; set; }
        public Exception exception { get; set; }
        public string message { get; set; }
    }
}
