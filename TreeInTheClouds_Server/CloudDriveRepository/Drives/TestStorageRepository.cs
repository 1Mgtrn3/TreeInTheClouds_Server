using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading.Tasks;
using TreeInTheClouds_Server.CloudDriveRepository.Enums;
using TreeInTheClouds_Server.CloudDriveRepository.OperationResults;

namespace TreeInTheClouds_Server.CloudDriveRepository.Drives
{
    class TestStorageRepository : ICloudDriveRepository
    {
        public TestStorageRepository(List<FileProps> previousFilesList)
        {
            GetPreviousFiles(previousFilesList);
        }

        public TestStorageRepository()
        {
            AuthStatus = Authenticate();
        }
        public string Path { get; set; }
        public AuthStatus AuthStatus { get; set; }

        public FileProps fileProps { get; set; }
        //public bool IsFilePresent { get ; set ; }

        public AuthStatus Authenticate()
        {
            return AuthStatus.OK;
        }

        public byte[] ReadFile()
        {
            var content = File.ReadAllBytes(Path);
            return content;
        }



        public string WriteFile(byte[] content)
        {
            File.WriteAllBytes(Path, content);
            return Path;
        }

        public OperationResultRead TryReadFile(int attempts)
        {
            int currentAttempt = 0;
            while (currentAttempt < attempts)
            {

                switch (AuthStatus)
                {
                    case AuthStatus.OK:

                        return new OperationResultRead() { operationResult = OperationStatusCode.SUCCESS, payload = ReadFile() };//ReadFile();


                    case AuthStatus.NOAU:
                        currentAttempt = attempts;
                        break;
                    case AuthStatus.RETRY:
                        Authenticate();

                        break;
                    default:
                        break;
                }
                ++currentAttempt;

            }
            return new OperationResultRead() { operationResult = OperationStatusCode.FAIL, exception = new AuthenticationException("Unable to authenticate") };

        }

        public OperationResult TryWriteFile(byte[] content, int attempts)
        {
            //throw new NotImplementedException();
            int currentAttempt = 0;
            while (currentAttempt < attempts)
            {
                switch (AuthStatus)
                {
                    case AuthStatus.OK:

                        //File.WriteAllBytes(pathTo, drive.ReadFile());
                        WriteFile(content);
                        currentAttempt = attempts;

                        return new OperationResult() { operationResult = OperationStatusCode.SUCCESS };
                    case AuthStatus.NOAU:
                        currentAttempt = attempts;
                        break;

                    case AuthStatus.RETRY:
                        AuthStatus = Authenticate();

                        break;
                    default:
                        break;
                }
                ++currentAttempt;
            }
            return new OperationResult() { operationResult = OperationStatusCode.FAIL, exception = new AuthenticationException("Unable to authenticate") };
            //if (AuthStatus ==AuthStatus.OK)
            //{
            //    return Path;
            //}
            //else
            //{
            //    return "";
            //}

        }

        public void SaveConfig()
        {


            string json = JsonConvert.SerializeObject(fileProps.Props, Formatting.Indented);
            File.WriteAllText("TestStrorageConfig.json", json);
        }

        public void LoadConfig()
        {
            string json = File.ReadAllText("TestStrorageConfig.json");
            fileProps.Props = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }

        public OperationResultCreate TryCreateFile(byte[] content, int attempts)
        {
            throw new NotImplementedException();
        }

        public OperationResult TryDeleteFile(int attempts)
        {
            throw new NotImplementedException();
        }

        public bool CheckIfConfigPresent()
        {
            throw new NotImplementedException();
        }

        public bool CheckIfAnyConfigPresent()
        {
            throw new NotImplementedException();
        }

        public void GetPreviousFiles(List<FileProps> resultList)
        {
            throw new NotImplementedException();
        }
    }
}
