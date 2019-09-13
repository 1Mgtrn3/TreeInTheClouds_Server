using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeInTheClouds_Server.CloudDriveRepository.Enums;
using TreeInTheClouds_Server.CloudDriveRepository.OperationResults;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Text;
using System.Security.Authentication;
using System.Web;
using Newtonsoft.Json;

namespace TreeInTheClouds_Server.CloudDriveRepository.Drives
{
    class GoogleDriveRepository : ICloudDriveRepository
    {

        public AuthStatus AuthStatus { get; set; }
        public FileProps fileProps { get; set; }
        



        UserCredential credential;
        DriveService driveService;
        string idField = "Id";
        string configFileName = "GoogleDriveConfig";

        public GoogleDriveRepository(List<FileProps> previousFilesList)
        {
            GetPreviousFiles(previousFilesList);
        }

        public GoogleDriveRepository(string directory, string fileName)
        {
            fileProps = new FileProps();
            fileProps.Directory = directory;
            fileProps.FileName = fileName;
            this.AuthStatus = Authenticate();
            fileProps.Directory = directory;
            fileProps.FileName = fileName;
            fileProps.IsFilePresent = CheckIfFilePresent();
            
        }

        #region Auth
        public AuthStatus Authenticate()
        {
            string[] Scopes = { DriveService.Scope.DriveFile }; //Permissions
            string ApplicationName = "CherryTreeCloudWrapper";



            using (var stream =
                new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = "token.json";
                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                
            }

            driveService = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            try
            {
                TestAuth();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception occured during authentication");
                Debug.WriteLine(ex.Message);
                return AuthStatus.NOAU;

            }

            return AuthStatus.OK;
        }


        private void TestAuth()
        {

            byte[] testBytes = Encoding.UTF8.GetBytes("testBytes");
            var realFileName = fileProps.FileName;
            fileProps.FileName = "authTest.txt";
            OperationResultCreate result = PostFile(testBytes);
            if (result.operationResult == OperationStatusCode.FAIL)
            {
                throw result.exception;
            }

            fileProps.Props[idField] = result.payload;


            var fileMetadata = driveService.Files.Get(fileProps.Props[idField]).Execute();
            if (fileMetadata.MimeType != "text/plain")
            {

                fileProps.FileName = realFileName;

                throw new AuthenticationException("Authentification failed");
            }
            DeleteFile();
            fileProps.FileName = realFileName;
            fileProps.Props[idField] = "";
        }

        #endregion
        public string GetIdFromPath(string shareLink)
        { //https://drive.google.com/open?id=1nq4vES61bNzQNVc6SeYve7Q3t9WCKEAM
            Uri shareUri = new Uri(shareLink);
            string id = HttpUtility.ParseQueryString(shareUri.Query).Get("id");
            return id;
        }

        public string GetPathFromId(string id)
        {
            return "https://drive.google.com/open?id=" + id;

        }

        public bool CheckIfAnyConfigPresent()
        {

            fileProps.IsConfigPresent =
                Directory.EnumerateFiles(Directory.GetCurrentDirectory(), $"{configFileName}*").Any();

            
            return fileProps.IsConfigPresent;

        }

        public bool CheckIfConfigPresent()
        {



            fileProps.IsConfigPresent = File.Exists($"{configFileName}{fileProps.FileName}.json");
            return fileProps.IsConfigPresent;

        }

        public bool CheckIfFilePresent()
        {
            if (CheckIfConfigPresent())
            {
                LoadConfig();
                if (fileProps.Props[idField] != "")
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
            else
            {
                GetIdByName(fileProps.FileName);

                if (fileProps.Props[idField] != "")
                {
                    return true;
                }
                else
                {
                    return false;

                }
            }
            //

        }

        public void GetIdByName(string name)
        {
            /* return ""*/
            fileProps.Props[idField] = "";

            //ChildrenResource.ListRequest 

            var request = driveService.Files.List();
            request.Fields = "nextPageToken, files(createdTime, id, name, parents)";
            IList<Google.Apis.Drive.v3.Data.File> files = request.Execute().Files;


            if (files != null && files.Count > 0)
            {
                var fileFound = files.Where(x => x.Parents.Contains(GetIdFromPath(fileProps.Directory)) && x.Name == name).FirstOrDefault();
                if (fileFound != default(Google.Apis.Drive.v3.Data.File))
                {
                    fileProps.Props[idField] = fileFound.Id;
                }
               

            }

        }

        #region CRUD
        private OperationResultCreate PostFile(byte[] content)
        {
            Debug.WriteLine($"Post file started. FileName: {fileProps.FileName} Directory: {fileProps.Directory}");
            

            Google.Apis.Drive.v3.Data.File fileMetadata;
            
            fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                
                Name = fileProps.FileName,
                Parents = new List<string>
                    {
                        GetIdFromPath(fileProps.Directory)
                    }
            };

            FilesResource.CreateMediaUpload request;

            //request = driveService.Files.Create()
            using (var stream = new MemoryStream(content))
            {

                request = driveService.Files.Create(fileMetadata, stream, "");

                request.Fields = "id";
                try
                {

                    request.Upload();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception occured during file upload");
                    Debug.WriteLine(ex.Message);
                    return new OperationResultCreate() { exception = ex, operationResult = OperationStatusCode.FAIL };
                }
            }
            var fileResponse = request.ResponseBody;
            fileProps.Props[idField] = fileResponse.Id;
            //fileProps.Directory = GetPathFromId(fileProps.Props[idField]);//todo: need to reconsider what to store in directory
            fileProps.IsFilePresent = true;
            return new OperationResultCreate() { operationResult = OperationStatusCode.SUCCESS, payload = fileResponse.Id };//fileResponse.Id;
        }



        public OperationResultRead GetFile()
        {
            var fileId = fileProps.Props[idField];
            Debug.WriteLine($"GetFile started. fileId: {fileId}");
            var request = driveService.Files.Get(fileId);
            using (var stream = new System.IO.MemoryStream())
            {
                #region progress
                 
                #endregion
                try
                {
                    request.DownloadWithStatus(stream);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception occured during file download");
                    Debug.WriteLine(ex.Message);
                    return new OperationResultRead() { operationResult = OperationStatusCode.FAIL, exception = ex };
                }
                #region progress2
                
                #endregion
                return new OperationResultRead() { operationResult = OperationStatusCode.SUCCESS, payload = stream.ToArray() };
            }
        }


        public OperationResult UpdateFile(byte[] content)
        {
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                

            };

            FilesResource.UpdateMediaUpload request;

            //request = driveService.Files.Create()
            using (var stream = new MemoryStream(content))
            {

                request = driveService.Files.Update(fileMetadata, fileProps.Props[idField], stream, "");


                //request.Fields = "id";
                try
                {
                    request.Upload();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception occured during file update");
                    Debug.WriteLine(ex.Message);
                    return new OperationResult() { operationResult = OperationStatusCode.FAIL, exception = ex };
                }

            }
            //var fileResponse = request.ResponseBody;
            return new OperationResult() { operationResult = OperationStatusCode.SUCCESS };
            
        }


        private OperationResult DeleteFile()
        {
            FilesResource.DeleteRequest request;

            
            request = driveService.Files.Delete(fileProps.Props[idField]);

            try
            {
                request.Execute();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception occured during file removal");
                Debug.WriteLine(ex.Message);
                return new OperationResult() { operationResult = OperationStatusCode.FAIL, exception = ex };
            }

            fileProps.IsFilePresent = false;
            return new OperationResult() { operationResult = OperationStatusCode.SUCCESS };
        }

        #endregion



        #region TRYs

        public OperationResultCreate TryCreateFile(byte[] content, int attempts)
        {
            int currentAttempt = 0;
            while (currentAttempt < attempts)
            {

                switch (AuthStatus)
                {
                    case AuthStatus.OK:

                        var result = PostFile(content);




                        if (result.operationResult == OperationStatusCode.SUCCESS)
                        {
                            SaveConfig();
                            return result;
                        }
                        else
                        {
                            return result;
                        }

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
            return new OperationResultCreate() { operationResult = OperationStatusCode.FAIL, exception = new AuthenticationException("Unable to authenticate") };
        }




        public OperationResult TryWriteFile(byte[] content, int attempts)
        {
            int currentAttempt = 0;
            while (currentAttempt < attempts)
            {

                switch (AuthStatus)
                {
                    case AuthStatus.OK:

                        var result = UpdateFile(content);
                        
                        return result;
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
            return new OperationResult() { operationResult = OperationStatusCode.FAIL, exception = new AuthenticationException("Unable to authenticate") };
        }

        public OperationResultRead TryReadFile(int attempts)
        {
            int currentAttempt = 0;
            while (currentAttempt < attempts)
            {

                switch (AuthStatus)
                {
                    case AuthStatus.OK:

                        var result = GetFile();
                        //return result;
                        if (result.operationResult == OperationStatusCode.SUCCESS)
                        {
                            SaveConfig();
                            return result;
                        }
                        else
                        {
                            return result;
                        }
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
            //listRequest.

        }





        public OperationResult TryDeleteFile(int attempts)
        {
            int currentAttempt = 0;
            while (currentAttempt < attempts)
            {

                switch (AuthStatus)
                {
                    case AuthStatus.OK:

                        var result = DeleteFile();
                        if (result.operationResult == OperationStatusCode.SUCCESS)
                        {
                            SaveConfig();
                            return result;

                        }
                        else
                        {
                            return result;
                        }

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
            return new OperationResult() { operationResult = OperationStatusCode.FAIL, exception = new AuthenticationException("Unable to authenticate") };

            


        }
        #endregion

        #region config


        public void SaveConfig()
        {
            string json = JsonConvert.SerializeObject(fileProps.Props, Formatting.Indented);
            File.WriteAllText($"{configFileName}{fileProps.FileName}.json", json);
        }

        public void LoadConfig()
        {

            string json = File.ReadAllText($"{configFileName}{fileProps.FileName}.json");
            fileProps.Props = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        }

        public void GetPreviousFiles(List<FileProps> resultList)
        {
            //resultList = new List<string>();
            var configFiles = Directory.EnumerateFiles(Directory.GetCurrentDirectory(), $"{configFileName}*");
            foreach (var configFile in configFiles)
            {
                
                string json = File.ReadAllText(configFile);
                var configProps = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                FileProps configFileProps = new FileProps(isFilePresent: true);
                configFileProps.Props = configProps;
                resultList.Add(configFileProps);
            }
            
        }
        #endregion
    }
}
