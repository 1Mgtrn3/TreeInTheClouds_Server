using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TreeInTheClouds_Server.CloudDriveRepository.OperationResults
{
    public class OperationResultRead : OperationResult
    {
        public byte[] payload { get; set; }
    }
}
