using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeInTheClouds_Server.CloudDriveRepository.Enums;
using TreeInTheClouds_Server.CloudDriveRepository.OperationResults;

namespace TreeInTheClouds_Server.CloudDriveRepository.Drives
{
    public interface ICloudDriveRepository
    {
        FileProps fileProps { get; set; }
        AuthStatus Authenticate();
        AuthStatus AuthStatus { get; set; }


        OperationResultCreate TryCreateFile(byte[] content, int attempts);
        OperationResult TryDeleteFile(int attempts);
        OperationResult TryWriteFile(byte[] content, int attempts);
        OperationResultRead TryReadFile(int attempts);
        void SaveConfig();
        bool CheckIfAnyConfigPresent();
        void GetPreviousFiles(List<FileProps> resultList); //fetches all the configuration files and filenames for all of them
        void LoadConfig();
    }
}
