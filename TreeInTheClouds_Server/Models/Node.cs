using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TreeInTheClouds_Server.Models
{
    public class Node
    {
        public int Id { get; set; }
        public int ParentId { get; set; }
        public int Sequence { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        

        public DateTime DateTime_Created { get; set; }
        public DateTime DateTime_LastSaved { get; set; }

        public string[] Tags { get; set; }

        public List<Image> Images { get; set; }
    }
}
