using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TreeInTheClouds_Server.CloudDriveRepository.Drives
{
    public class FileProps
    {
        public FileProps(bool isFilePresent)
        {
            Props = new Dictionary<string, string>();
            IsFilePresent = isFilePresent;
        }
        public FileProps()
        {
            Props = new Dictionary<string, string>();
            IsFilePresent = false;
        }
        //public string Path { get { return Props["Path"]; } set { Props["Path"] = value; } }
        public string Directory { get { return Props["Directory"]; } set { Props["Directory"] = value; } }
        public string FileName { get { return Props["FileName"]; } set { Props["FileName"] = value; } }
        public bool IsConfigPresent { get; set; }
        public bool IsFilePresent
        {
            get
            {
                //if (Props["IsFilePresent"]=="")
                //{
                //    return false;
                //}
                //else
                //{
                //    return true;
                //}
                return Props["IsFilePresent"] == "1";

            }

            set
            {
                if (value)
                {
                    Props["IsFilePresent"] = "1";
                }
                else
                {
                    Props["IsFilePresent"] = "0";
                }
            }

        }
        public Dictionary<String, String> Props
        {
            get; set;
        }
    }
}
