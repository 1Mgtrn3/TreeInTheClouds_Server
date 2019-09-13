using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TreeInTheClouds_Server.CloudDriveRepository.Enums;

namespace TreeInTheClouds_Server.CloudDriveRepository.Drives
{
    public static class CloudDriveProviderFactory
    {
        public static ICloudDriveRepository GetCloudDrive(CloudDrive cloudDrive, string directory, string fileName)
        {
            switch (cloudDrive)
            {
                case CloudDrive.GoogleDrive:
                    return new GoogleDriveRepository(directory, fileName);

                case CloudDrive.TestStorage:
                    return new TestStorageRepository();
                default:
                    return new TestStorageRepository();
            }

        }

        public static void GetPreviousFiles(CloudDrive cloudDrive, List<FileProps> previousFilesPropsList)
        {

            switch (cloudDrive)
            {
                case CloudDrive.GoogleDrive:
                    var googleDrive = new GoogleDriveRepository(previousFilesPropsList);
                    break;
                case CloudDrive.TestStorage:
                    var testStorageDrive = new TestStorageRepository(previousFilesPropsList);
                    break;
                default:
                    var testStorageDrive2 = new TestStorageRepository(previousFilesPropsList);
                    break;
            }
        }

        

    }
}
